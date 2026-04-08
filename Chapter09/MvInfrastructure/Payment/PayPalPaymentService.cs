using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MvApplication.Ports;
using MvDomain.Entities;
using MvInfrastructure.Configuration;
using MvInfrastructure.Exceptions;

namespace MvInfrastructure.Payment;

public class PayPalPaymentService(
  IHttpClientFactory httpClientFactory,
  IOptions<PayPalOptions> options
) : IPaymentService {
  private readonly PayPalOptions _options = options.Value;
  private static readonly JsonSerializerOptions JsonOptions = new() {
    PropertyNameCaseInsensitive = true
  };

  public async Task<PaymentCheckoutResult> CreateCheckoutAsync(TicketOrder order, CancellationToken ct = default) {
    ValidateConfiguration();

    var accessToken = await GetAccessTokenAsync(ct);
    var client = httpClientFactory.CreateClient("PayPalApi");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var payload = new {
      intent = "CAPTURE",
      purchase_units = new[] {
        new {
          reference_id = order.Id.ToString(),
          custom_id = order.Id.ToString(),
          description = $"Ve su kien {order.EventName}",
          amount = new {
            currency_code = NormalizePayPalCurrency(_options.Currency),
            value = order.TotalAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
          }
        }
      },
      application_context = new {
        brand_name = _options.BrandName,
        user_action = "PAY_NOW",
        return_url = BuildUrl(_options.ReturnUrl, new Dictionary<string, string> {
          ["orderId"] = order.Id.ToString()
        }),
        cancel_url = BuildUrl(_options.CancelUrl, new Dictionary<string, string> {
          ["orderId"] = order.Id.ToString()
        })
      }
    };

    using var response = await client.PostAsync(
      "/v2/checkout/orders",
      new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
      ct);

    var body = await response.Content.ReadAsStringAsync(ct);
    if (!response.IsSuccessStatusCode) {
      throw new InfrastructureException($"PayPal tao order that bai: {(int)response.StatusCode} - {body}");
    }

    var payPalOrder = JsonSerializer.Deserialize<PayPalOrderResponse>(body, JsonOptions)
                      ?? throw new InfrastructureException("Khong doc duoc phan hoi tao order PayPal.");

    var approvalUrl = payPalOrder.Links?.FirstOrDefault(x => x.Rel is "approve" or "payer-action")?.Href;
    if (string.IsNullOrWhiteSpace(approvalUrl)) {
      throw new InfrastructureException("Khong tim thay approval url tu PayPal.");
    }

    return new PaymentCheckoutResult(approvalUrl, payPalOrder.Id);
  }

  public async Task<PaymentVerificationResult> VerifyReturnAsync(TicketOrder order, Dictionary<string, string> callbackData, CancellationToken ct = default) {
    ValidateConfiguration();

    if (!callbackData.TryGetValue("token", out var token) || string.IsNullOrWhiteSpace(token)) {
      return new PaymentVerificationResult(false, null, "Thieu token tu PayPal.");
    }

    var accessToken = await GetAccessTokenAsync(ct);
    var client = httpClientFactory.CreateClient("PayPalApi");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    using var response = await client.PostAsync(
      $"/v2/checkout/orders/{token}/capture",
      new StringContent("{}", Encoding.UTF8, "application/json"),
      ct);
    var body = await response.Content.ReadAsStringAsync(ct);

    if (!response.IsSuccessStatusCode) {
      throw new InfrastructureException($"PayPal capture that bai: {(int)response.StatusCode} - {body}");
    }

    var captureResponse = JsonSerializer.Deserialize<PayPalCaptureResponse>(body, JsonOptions)
                          ?? throw new InfrastructureException("Khong doc duoc phan hoi capture PayPal.");

    var captureId = captureResponse.PurchaseUnits?
      .FirstOrDefault()?
      .Payments?
      .Captures?
      .FirstOrDefault()?
      .Id;

    var isSuccess = string.Equals(captureResponse.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase)
                    || captureResponse.PurchaseUnits?.Any(x => x.Payments?.Captures?.Any(c => string.Equals(c.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase)) == true) == true;

    return isSuccess
      ? new PaymentVerificationResult(true, captureId ?? token, "PayPal da xac nhan thanh toan thanh cong.")
      : new PaymentVerificationResult(false, null, $"PayPal chua xac nhan thanh toan. status={captureResponse.Status}.");
  }

  public async Task<PaymentWebhookResult?> ParseWebhookAsync(string payload, Dictionary<string, string> headers, CancellationToken ct = default) {
    ValidateConfiguration();

    using var document = JsonDocument.Parse(payload);
    var root = document.RootElement;

    if (!root.TryGetProperty("event_type", out var eventTypeElement)) {
      return null;
    }

    var eventType = eventTypeElement.GetString();
    if (string.IsNullOrWhiteSpace(eventType)) {
      return null;
    }

    if (!root.TryGetProperty("resource", out var resource)) {
      return null;
    }

    string? gatewayReferenceId = null;
    Guid? orderId = null;

    if (resource.TryGetProperty("supplementary_data", out var supplementaryData)
        && supplementaryData.TryGetProperty("related_ids", out var relatedIds)
        && relatedIds.TryGetProperty("order_id", out var orderIdElementFromRelated)) {
      gatewayReferenceId = orderIdElementFromRelated.GetString();
    }

    if (resource.TryGetProperty("id", out var resourceIdElement) && string.IsNullOrWhiteSpace(gatewayReferenceId)) {
      gatewayReferenceId = resourceIdElement.GetString();
    }

    if (resource.TryGetProperty("custom_id", out var customIdElement)
        && Guid.TryParse(customIdElement.GetString(), out var customOrderId)) {
      orderId = customOrderId;
    }

    if (!orderId.HasValue && !string.IsNullOrWhiteSpace(gatewayReferenceId)) {
      orderId = await ResolveOrderIdFromPayPalOrderAsync(gatewayReferenceId, ct);
    }

    if (!orderId.HasValue && resource.TryGetProperty("purchase_units", out var purchaseUnitsElement) && purchaseUnitsElement.ValueKind == JsonValueKind.Array) {
      foreach (var purchaseUnit in purchaseUnitsElement.EnumerateArray()) {
        if (purchaseUnit.TryGetProperty("custom_id", out var purchaseUnitCustomId)
            && Guid.TryParse(purchaseUnitCustomId.GetString(), out var purchaseUnitOrderId)) {
          orderId = purchaseUnitOrderId;
          break;
        }
      }
    }

    var isSuccess = eventType.Contains("COMPLETED", StringComparison.OrdinalIgnoreCase)
                    || eventType.Contains("APPROVED", StringComparison.OrdinalIgnoreCase);

    var message = isSuccess
      ? "Webhook PayPal xac nhan thanh toan thanh cong."
      : $"Webhook PayPal tra ve su kien {eventType}.";

    string? captureId = null;
    if (resource.TryGetProperty("id", out var captureElement) && isSuccess) {
      captureId = captureElement.GetString();
    }

    return new PaymentWebhookResult(orderId, gatewayReferenceId, isSuccess, captureId ?? gatewayReferenceId, message);
  }

  private async Task<string> GetAccessTokenAsync(CancellationToken ct) {
    var client = httpClientFactory.CreateClient("PayPalApi");
    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.Secret}"));

    using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
      ["grant_type"] = "client_credentials"
    });

    using var response = await client.SendAsync(request, ct);
    var body = await response.Content.ReadAsStringAsync(ct);

    if (!response.IsSuccessStatusCode) {
      throw new InfrastructureException($"Khong lay duoc access token PayPal: {(int)response.StatusCode} - {body}");
    }

    using var tokenDocument = JsonDocument.Parse(body);
    return tokenDocument.RootElement.GetProperty("access_token").GetString()
           ?? throw new InfrastructureException("PayPal khong tra ve access token.");
  }

  private async Task<Guid?> ResolveOrderIdFromPayPalOrderAsync(string payPalOrderId, CancellationToken ct) {
    var accessToken = await GetAccessTokenAsync(ct);
    var client = httpClientFactory.CreateClient("PayPalApi");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    using var response = await client.GetAsync($"/v2/checkout/orders/{payPalOrderId}", ct);
    if (!response.IsSuccessStatusCode) {
      return null;
    }

    var body = await response.Content.ReadAsStringAsync(ct);
    using var document = JsonDocument.Parse(body);
    if (document.RootElement.TryGetProperty("purchase_units", out var purchaseUnits)
        && purchaseUnits.ValueKind == JsonValueKind.Array) {
      foreach (var purchaseUnit in purchaseUnits.EnumerateArray()) {
        if (purchaseUnit.TryGetProperty("custom_id", out var customId)
            && Guid.TryParse(customId.GetString(), out var orderId)) {
          return orderId;
        }
      }
    }

    return null;
  }

  private void ValidateConfiguration() {
    if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.Secret)) {
      throw new InfrastructureException("PayPal chua duoc cau hinh ClientId/Secret.");
    }
  }

  private static string BuildUrl(string baseUrl, Dictionary<string, string> query) {
    var separator = baseUrl.Contains('?') ? '&' : '?';
    var queryString = string.Join("&", query.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
    return $"{baseUrl}{separator}{queryString}";
  }

  private static string NormalizePayPalCurrency(string currency) {
    var normalized = (currency ?? string.Empty).Trim().ToUpperInvariant();

    if (normalized.Contains("VN")) {
      return "VND";
    }

    if (normalized.Contains("US")) {
      return "USD";
    }

    return normalized;
  }

  private sealed class PayPalOrderResponse {
    public string Id { get; set; } = string.Empty;
    public List<PayPalLink>? Links { get; set; }
  }

  private sealed class PayPalCaptureResponse {
    public string Status { get; set; } = string.Empty;
    public List<PayPalPurchaseUnit>? PurchaseUnits { get; set; }
  }

  private sealed class PayPalPurchaseUnit {
    public PayPalPayments? Payments { get; set; }
  }

  private sealed class PayPalPayments {
    public List<PayPalCapture>? Captures { get; set; }
  }

  private sealed class PayPalCapture {
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
  }

  private sealed class PayPalLink {
    public string Href { get; set; } = string.Empty;
    public string Rel { get; set; } = string.Empty;
  }
}

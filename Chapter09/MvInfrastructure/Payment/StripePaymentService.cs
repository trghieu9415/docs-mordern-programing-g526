using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using MvApplication.Ports;
using MvDomain.Entities;
using MvInfrastructure.Configuration;
using MvInfrastructure.Exceptions;
using StripeEvent = Stripe.Event;
using StripeCheckoutSession = Stripe.Checkout.Session;

namespace MvInfrastructure.Payment;

public class StripePaymentService(IOptions<StripeOptions> options) : IPaymentService {
  private readonly StripeOptions _options = options.Value;

  public async Task<PaymentCheckoutResult> CreateCheckoutAsync(TicketOrder order, CancellationToken ct = default) {
    ValidateConfiguration();

    StripeConfiguration.ApiKey = _options.SecretKey;

    var service = new SessionService();
    var successUrl = BuildStripeSuccessUrl(_options.SuccessUrl, order.Id);
    var cancelUrl = BuildUrl(_options.CancelUrl, new Dictionary<string, string> {
      ["orderId"] = order.Id.ToString()
    });

    var session = await service.CreateAsync(new SessionCreateOptions {
      Mode = "payment",
      SuccessUrl = successUrl,
      CancelUrl = cancelUrl,
      ClientReferenceId = order.Id.ToString(),
      PaymentMethodTypes = ["card"],
      Metadata = new Dictionary<string, string> {
        ["orderId"] = order.Id.ToString(),
        ["customerEmail"] = order.CustomerEmail
      },
      LineItems = [
        new SessionLineItemOptions {
          Quantity = order.Quantity,
          PriceData = new SessionLineItemPriceDataOptions {
            Currency = NormalizeStripeCurrency(_options.Currency),
            UnitAmountDecimal = order.TotalAmount / order.Quantity * 100,
            ProductData = new SessionLineItemPriceDataProductDataOptions {
              Name = $"Ve su kien {order.EventName}"
            }
          }
        }
      ]
    }, cancellationToken: ct);

    return new PaymentCheckoutResult(session.Url ?? string.Empty, session.Id);
  }

  public async Task<PaymentVerificationResult> VerifyReturnAsync(TicketOrder order, Dictionary<string, string> callbackData, CancellationToken ct = default) {
    ValidateConfiguration();

    StripeConfiguration.ApiKey = _options.SecretKey;

    if (!callbackData.TryGetValue("sessionId", out var sessionId) && !callbackData.TryGetValue("session_id", out sessionId)) {
      return new PaymentVerificationResult(false, null, "Thieu sessionId tu Stripe.");
    }

    if (string.Equals(sessionId, "{CHECKOUT_SESSION_ID}", StringComparison.OrdinalIgnoreCase)) {
      return new PaymentVerificationResult(false, null, "Stripe chua thay sessionId that. Hay thanh toan tu trang checkout va de Stripe redirect ve return URL.");
    }

    var session = await new SessionService().GetAsync(sessionId, cancellationToken: ct);
    var isSuccess = string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(session.Status, "complete", StringComparison.OrdinalIgnoreCase);

    return isSuccess
      ? new PaymentVerificationResult(true, session.PaymentIntentId ?? session.Id, "Stripe da xac nhan thanh toan thanh cong.")
      : new PaymentVerificationResult(false, null, $"Stripe chua xac nhan thanh toan. payment_status={session.PaymentStatus}, session_status={session.Status}.");
  }

  public Task<PaymentWebhookResult?> ParseWebhookAsync(string payload, Dictionary<string, string> headers, CancellationToken ct = default) {
    ValidateConfiguration();

    StripeEvent stripeEvent;
    if (headers.TryGetValue("Stripe-Signature", out var signature) && !string.IsNullOrWhiteSpace(_options.WebhookSecret)) {
      stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret);
    } else {
      stripeEvent = EventUtility.ParseEvent(payload);
    }

    var isRelevant = stripeEvent.Type is "checkout.session.completed" or "checkout.session.async_payment_succeeded" or "checkout.session.async_payment_failed" or "checkout.session.expired";
    if (!isRelevant) {
      return Task.FromResult<PaymentWebhookResult?>(null);
    }

    var session = stripeEvent.Data.Object as StripeCheckoutSession;
    if (session is null) {
      return Task.FromResult<PaymentWebhookResult?>(null);
    }

    Guid? orderId = null;
    if (Guid.TryParse(session.ClientReferenceId ?? session.Metadata.GetValueOrDefault("orderId"), out var parsedOrderId)) {
      orderId = parsedOrderId;
    }

    var isSuccess = stripeEvent.Type is "checkout.session.completed" or "checkout.session.async_payment_succeeded";
    var message = isSuccess
      ? "Webhook Stripe xac nhan thanh toan thanh cong."
      : "Webhook Stripe thong bao thanh toan that bai hoac het han.";

    return Task.FromResult<PaymentWebhookResult?>(new PaymentWebhookResult(
      orderId,
      session.Id,
      isSuccess,
      session.PaymentIntentId ?? session.Id,
      message));
  }

  private void ValidateConfiguration() {
    if (string.IsNullOrWhiteSpace(_options.SecretKey)) {
      throw new InfrastructureException("Stripe:SecretKey chua duoc cau hinh.");
    }
  }

  private static string BuildUrl(string baseUrl, Dictionary<string, string> query) {
    var separator = baseUrl.Contains('?') ? '&' : '?';
    var queryString = string.Join("&", query.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
    return $"{baseUrl}{separator}{queryString}";
  }

  private static string BuildStripeSuccessUrl(string baseUrl, Guid orderId) {
    var separator = baseUrl.Contains('?') ? '&' : '?';
    return $"{baseUrl}{separator}orderId={Uri.EscapeDataString(orderId.ToString())}&sessionId={{CHECKOUT_SESSION_ID}}";
  }

  private static string NormalizeStripeCurrency(string currency) {
    var normalized = (currency ?? string.Empty).Trim().ToLowerInvariant();

    if (normalized.Contains("vn")) {
      return "vnd";
    }

    if (normalized.Contains("us")) {
      return "usd";
    }

    return normalized;
  }
}


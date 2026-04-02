namespace MvInfrastructure.Configuration;

public class PayPalOptions {
  public const string SectionName = "PayPal";

  public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
  public string ClientId { get; set; } = string.Empty;
  public string Secret { get; set; } = string.Empty;
  public string ReturnUrl { get; set; } = "https://localhost:7225/api/ticket-orders/return";
  public string CancelUrl { get; set; } = "https://localhost:7225/api/ticket-orders/cancel";
  public string Currency { get; set; } = "USD";
  public string BrandName { get; set; } = "Event Ticketing System";
  public string? WebhookId { get; set; }
}

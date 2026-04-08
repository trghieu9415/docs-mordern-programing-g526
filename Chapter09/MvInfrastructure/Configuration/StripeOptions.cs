namespace MvInfrastructure.Configuration;

public class StripeOptions {
  public const string SectionName = "Stripe";

  public string SecretKey { get; set; } = string.Empty;
  public string WebhookSecret { get; set; } = string.Empty;
  public string Currency { get; set; } = "usd";
  public string SuccessUrl { get; set; } = "https://localhost:7225/api/ticket-orders/return";
  public string CancelUrl { get; set; } = "https://localhost:7225/api/ticket-orders/cancel";
}

using MvDomain.Entities;

namespace MvApplication.Ports;

public interface IPaymentService {
  Task<PaymentCheckoutResult> CreateCheckoutAsync(TicketOrder order, CancellationToken ct = default);
  Task<PaymentVerificationResult> VerifyReturnAsync(TicketOrder order, Dictionary<string, string> callbackData, CancellationToken ct = default);
  Task<PaymentWebhookResult?> ParseWebhookAsync(string payload, Dictionary<string, string> headers, CancellationToken ct = default);
}

public record PaymentCheckoutResult(
  string PaymentUrl,
  string? GatewayReferenceId
);

public record PaymentVerificationResult(
  bool IsSuccess,
  string? GatewayTransactionId,
  string? Message
);

public record PaymentWebhookResult(
  Guid? OrderId,
  string? GatewayReferenceId,
  bool IsSuccess,
  string? GatewayTransactionId,
  string? Message
);

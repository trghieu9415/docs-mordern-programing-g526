namespace MvApplication.UseCases.Orders.VerifyPaymentReturn;

public record VerifyPaymentReturnResult(
  Guid OrderId,
  bool IsSuccess,
  string Status,
  string? TicketCode,
  string? GatewayTransactionId,
  string? Message,
  bool IsDuplicate
);

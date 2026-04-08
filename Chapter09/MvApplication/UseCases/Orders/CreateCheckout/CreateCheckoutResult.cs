namespace MvApplication.UseCases.Orders.CreateCheckout;

public record CreateCheckoutResult(
  Guid OrderId,
  string PaymentUrl,
  string PaymentProvider,
  string Status
);

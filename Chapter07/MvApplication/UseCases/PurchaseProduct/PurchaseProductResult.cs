namespace MvApplication.UseCases.PurchaseProduct;

public record PurchaseProductResult(
  Guid OrderId,
  Guid ProductId,
  int Quantity,
  int RemainingStock
);

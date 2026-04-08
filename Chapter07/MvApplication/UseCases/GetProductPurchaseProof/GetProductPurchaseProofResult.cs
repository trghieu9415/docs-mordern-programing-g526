namespace MvApplication.UseCases.GetProductPurchaseProof;

public record OrderSummaryDto(
  Guid Id,
  Guid ProductId,
  string UserId,
  int Quantity,
  DateTime CreatedAt
);

/// <summary>Minh chung sau test dong thoi: ton kho va so don hang.</summary>
public record GetProductPurchaseProofResult(
  Guid ProductId,
  string ProductName,
  int AvailableStock,
  int OrderCount,
  IReadOnlyList<OrderSummaryDto> Orders
);

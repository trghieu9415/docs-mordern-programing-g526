using MediatR;
using MvApplication.Abstractions;

namespace MvApplication.UseCases.PurchaseProduct;

public record PurchaseProductCommand(
  Guid ProductId,
  string UserId,
  int Quantity
) : IRequest<PurchaseProductResult>, ILockable {
  public string LockKey => $"locks:product:{ProductId}";
  public TimeSpan WaitTime => TimeSpan.FromSeconds(1);
}

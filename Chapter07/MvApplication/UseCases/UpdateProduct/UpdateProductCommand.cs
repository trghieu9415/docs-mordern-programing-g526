using MvApplication.Abstractions;
using MediatR;

namespace MvApplication.UseCases.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, decimal Price, string? ImageUrl) : IRequest<Guid>, ILockable {
  public string LockKey => $"locks:product:{Id}";
  public TimeSpan WaitTime => TimeSpan.FromSeconds(5);
}

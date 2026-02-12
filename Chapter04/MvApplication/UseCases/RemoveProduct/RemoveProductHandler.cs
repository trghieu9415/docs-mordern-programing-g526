using MediatR;
using MvApplication.Ports;

namespace MvApplication.UseCases.RemoveProduct;

public class RemoveProductHandler(IProductManager manager) : IRequestHandler<RemoveProductCommand, Unit> {
  public async Task<Unit> Handle(RemoveProductCommand request, CancellationToken ct) {
    var product = await manager.GetByIdAsync(request.Id, ct);

    if (product == null) {
      throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID: {request.Id}");
    }

    await manager.DeleteAsync(request.Id, ct);
    return Unit.Value;
  }
}

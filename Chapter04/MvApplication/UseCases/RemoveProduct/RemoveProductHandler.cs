using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.RemoveProduct;

public class RemoveProductHandler(IProductManager productManager) : IRequestHandler<RemoveProductCommand, Unit> {
  public async Task<Unit> Handle(RemoveProductCommand request, CancellationToken ct) {
    var product =
      await productManager.GetByIdAsync(request.Id, ct)
      ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.Id}", 404);

    await productManager.DeleteAsync(request.Id, ct);
    return Unit.Value;
  }
}

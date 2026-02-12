using MediatR;
using MvApplication.Ports;

namespace MvApplication.UseCases.UpdateProduct;

public class UpdateProductHandler(IProductManager manager) : IRequestHandler<UpdateProductCommand, Unit> {
  public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken ct) {
    var product = await manager.GetByIdAsync(request.Id, ct);

    if (product == null) {
      throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID: {request.Id}");
    }

    product.Update(request.Name, request.Price, request.ImageUrl);
    await manager.UpdateAsync(product, ct);

    return Unit.Value;
  }
}

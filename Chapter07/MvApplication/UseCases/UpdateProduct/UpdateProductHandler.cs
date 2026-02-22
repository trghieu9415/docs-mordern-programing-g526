using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.UpdateProduct;

public class UpdateProductHandler(IProductManager productManager, ICacheStorage cache)
  : IRequestHandler<UpdateProductCommand, Guid> {
  public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken ct) {
    var product =
      await productManager.GetByIdAsync(request.Id, ct)
      ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.Id}", 404);

    product.Update(request.Name, request.Price, request.ImageUrl);
    await productManager.UpdateAsync(product, ct);

    await cache.RemoveAsync($"product:{request.Id}", ct);
    return product.Id;
  }
}

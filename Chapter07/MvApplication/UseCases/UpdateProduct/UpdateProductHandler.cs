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

    // Giả lập request chậm 5s để test distributed lock
    await Task.Delay(TimeSpan.FromSeconds(5), ct);

    product.Update(request.Name, request.Price, request.ImageUrl);
    await productManager.UpdateAsync(product, ct);

    // await cache.RemoveAsync($"product:{request.Id}", ct);
    // await cache.RemoveAsync("products:first20", ct);
    return product.Id;
  }
}

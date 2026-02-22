using MediatR;
using MvApplication.Ports;
using MvDomain.Entities;

namespace MvApplication.UseCases.CreateProduct;

public class CreateProductHandler(IProductManager productManager, ICacheStorage cache)
  : IRequestHandler<CreateProductCommand, Guid> {
  public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct) {
    var product = Product.Create(request.Name, request.Price, request.ImageUrl);
    await productManager.AddAsync(product, ct);
    await cache.RemoveAsync("products:paged:1:20", ct);
    await cache.RemoveAsync("products:paged:1:10", ct);
    return product.Id;
  }
}

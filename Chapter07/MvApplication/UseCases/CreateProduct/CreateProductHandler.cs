using MediatR;
using MvApplication.Ports;
using MvDomain.Entities;

namespace MvApplication.UseCases.CreateProduct;

public class CreateProductHandler(IProductManager productManager, ICacheStorage cache)
  : IRequestHandler<CreateProductCommand, Guid> {
  private const int CachedPageSize = 20;

  public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct) {
    var product = Product.Create(request.Name, request.Price, request.ImageUrl);
    await productManager.AddAsync(product, ct);

    var (firstProducts, _) = await productManager.GetPagedAsync(1, CachedPageSize, ct);
    var isInFirstPage = firstProducts.Any(p => p.Id == product.Id);

    if (isInFirstPage) {
      await cache.RemoveAsync("products:first20", ct);
    }

    return product.Id;
  }
}

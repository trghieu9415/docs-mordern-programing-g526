using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetProduct;

public class GetProductHandler(
  IProductManager productManager,
  ICacheStorage cache,
  IMapper mapper
) : IRequestHandler<GetProductQuery, GetProductResult> {
  private const string CacheKeyPrefix = "product";
  private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

  public async Task<GetProductResult> Handle(GetProductQuery request, CancellationToken ct) {
    var cacheKey = $"{CacheKeyPrefix}:{request.Id}";
    var cached = await cache.GetAsync<GetProductResult>(cacheKey, ct);
    if (cached != null)
      return cached;

    var product =
      await productManager.GetByIdAsync(request.Id, ct)
      ?? throw new AppException($"Không tìm thấy sản phẩm ID: {request.Id}", 404);

    var dto = mapper.Map<ProductDto>(product);
    var result = new GetProductResult(dto);
    await cache.SetAsync(cacheKey, result, CacheExpiration, ct);
    return result;
  }
}

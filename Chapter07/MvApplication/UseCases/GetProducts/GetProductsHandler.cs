using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Models;
using MvApplication.Options;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetProducts;

public class GetProductsHandler(
  IProductManager manager,
  ICacheStorage cache,
  ProductOptions options,
  IMapper mapper
) : IRequestHandler<GetProductsQuery, GetProductsResult> {
  private const string CacheKey = "products:first20";
  private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(2);
  private const int FixedPage = 1;
  private const int FixedPageSize = 20;

  public async Task<GetProductsResult> Handle(GetProductsQuery request, CancellationToken ct) {
    var cached = await cache.GetAsync<GetProductsResult>(CacheKey, ct);
    if (cached != null)
      return cached;

    var (items, total) = await manager.GetPagedAsync(FixedPage, FixedPageSize, ct);
    var dtos = mapper.Map<IList<ProductDto>>(items);
    var meta = Meta.Create(FixedPage, FixedPageSize, total);
    var result = new GetProductsResult(dtos, meta);
    await cache.SetAsync(CacheKey, result, CacheExpiration, ct);
    return result;
  }
}

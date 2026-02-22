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
  private const string CacheKeyPrefix = "products:paged";
  private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(2);

  public async Task<GetProductsResult> Handle(GetProductsQuery request, CancellationToken ct) {
    var pageSize = request.PageSize > 0 ? request.PageSize : options.DefaultItemsPerPage;
    if (pageSize > options.MaxItemsPerPage)
      pageSize = options.MaxItemsPerPage;
    var page = request.Page > pageSize ? pageSize : request.Page;

    var cacheKey = $"{CacheKeyPrefix}:{page}:{pageSize}";
    var cached = await cache.GetAsync<GetProductsResult>(cacheKey, ct);
    if (cached != null)
      return cached;

    var (items, total) = await manager.GetPagedAsync(page, pageSize, ct);
    var dtos = mapper.Map<IList<ProductDto>>(items);
    var meta = Meta.Create(request.Page, request.PageSize, total);
    var result = new GetProductsResult(dtos, meta);
    await cache.SetAsync(cacheKey, result, CacheExpiration, ct);
    return result;
  }
}

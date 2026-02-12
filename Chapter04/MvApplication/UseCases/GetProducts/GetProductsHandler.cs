using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Models;
using MvApplication.Options;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetProducts;

public class GetProductsHandler(IProductManager manager, ProductOptions options, IMapper mapper)
  : IRequestHandler<GetProductsQuery, GetProductsResult> {
  public async Task<GetProductsResult> Handle(GetProductsQuery request, CancellationToken ct) {
    var pageSize = request.PageSize > 0 ? request.PageSize : options.DefaultItemsPerPage;

    if (pageSize > options.MaxItemsPerPage) {
      pageSize = options.MaxItemsPerPage;
    }

    var page = request.Page > pageSize ? pageSize : request.Page;

    var (items, total) = await manager.GetPagedAsync(page, pageSize, ct);

    var dtos = mapper.Map<IList<ProductDto>>(items);
    var meta = Meta.Create(request.Page, request.PageSize, total);

    return new GetProductsResult(dtos, meta);
  }
}

using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Models;
using MvApplication.Options;
using MvApplication.Ports;
namespace MvApplication.UseCases.GetProductsByCategory;

public class GetProductsByCategoryHandler(IUnitOfWork uow, ProductOptions options, IMapper mapper)
  : IRequestHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult> {
  public async Task<GetProductsByCategoryResult> Handle(
    GetProductsByCategoryQuery request, CancellationToken ct) {
    var pageSize = request.PageSize > 0 ? request.PageSize : options.DefaultItemsPerPage;
    if (pageSize > options.MaxItemsPerPage)
      pageSize = options.MaxItemsPerPage;
    var page = request.Page > 0 ? request.Page : 1;
    var (items, total) = await uow.Products.GetPagedByCategoryAsync(
      request.CategoryId, page, pageSize, ct);
    var dtos = mapper.Map<IList<ProductWithCategoryDto>>(items);
    var meta = Meta.Create(page, pageSize, total);
    return new GetProductsByCategoryResult(dtos, meta);
  }
}

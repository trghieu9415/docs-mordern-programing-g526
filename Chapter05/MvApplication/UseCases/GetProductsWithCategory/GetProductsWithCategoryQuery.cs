using MediatR;

namespace MvApplication.UseCases.GetProductsWithCategory;


public record GetProductsWithCategoryQuery(int Page = 1, int PageSize = 10)
  : IRequest<GetProductsWithCategoryResult>;


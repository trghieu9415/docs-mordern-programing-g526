using MediatR;

namespace MvApplication.UseCases.GetProductsByCategory;


public record GetProductsByCategoryQuery(int CategoryId, int Page = 1, int PageSize = 10)
  : IRequest<GetProductsByCategoryResult>;


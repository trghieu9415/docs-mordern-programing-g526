using MediatR;

namespace MvApplication.UseCases.GetProducts;

public record GetProductsQuery(int Page = 1, int PageSize = 10) : IRequest<GetProductsResult>;

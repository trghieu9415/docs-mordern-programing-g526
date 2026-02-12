using MediatR;

namespace MvApplication.UseCases.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<GetProductResult>;

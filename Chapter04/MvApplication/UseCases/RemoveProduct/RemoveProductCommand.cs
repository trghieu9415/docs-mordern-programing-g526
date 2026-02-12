using MediatR;

namespace MvApplication.UseCases.RemoveProduct;

public record RemoveProductCommand(Guid Id) : IRequest<Unit>;

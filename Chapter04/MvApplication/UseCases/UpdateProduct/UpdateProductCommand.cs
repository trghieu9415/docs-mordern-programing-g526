using MediatR;

namespace MvApplication.UseCases.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, decimal Price, string? ImageUrl) : IRequest<Guid>;

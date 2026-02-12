using MediatR;

namespace MvApplication.UseCases.CreateProduct;

public record CreateProductCommand(
  string Name,
  decimal Price,
  string? ImageUrl
) : IRequest<Guid>;

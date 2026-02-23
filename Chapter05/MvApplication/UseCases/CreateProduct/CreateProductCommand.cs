﻿﻿using MediatR;

namespace MvApplication.UseCases.CreateProduct;

public record CreateProductCommand : IRequest<Guid> {
  public string Name { get; init; } = string.Empty;
  public decimal Price { get; init; }
  public string? ImageUrl { get; init; }
  public int? CategoryId { get; init; }
}

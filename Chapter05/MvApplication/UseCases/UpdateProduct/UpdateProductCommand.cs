﻿﻿using MediatR;

namespace MvApplication.UseCases.UpdateProduct;

public record UpdateProductCommand : IRequest<Guid> {
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public decimal Price { get; init; }
  public string? ImageUrl { get; init; }
  public int? CategoryId { get; init; }
}

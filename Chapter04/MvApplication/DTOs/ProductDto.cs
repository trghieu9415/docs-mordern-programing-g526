namespace MvApplication.DTOs;

public record ProductDto(
  Guid Id,
  string Name,
  decimal Price,
  int Stock,
  string? ImageUrl
);

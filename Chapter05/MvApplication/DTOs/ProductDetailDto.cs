namespace MvApplication.DTOs;

public record ProductDetailDto(
  Guid Id,
  string Name,
  decimal Price,
  int Stock,
  string? ImageUrl,
  string? CategoryName,
  IList<string> Tags,
  string? Description,
  string? Specification
);


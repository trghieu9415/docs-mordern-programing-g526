namespace MvApplication.DTOs;

public record ProductWithCategoryDto(
  Guid Id,
  string Name,
  decimal Price,
  int Stock,
  string? ImageUrl,
  string? CategoryName
);


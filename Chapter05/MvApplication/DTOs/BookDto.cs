namespace MvApplication.DTOs;

public record BookDto(
  int Id,
  string Title,
  IReadOnlyCollection<CategoryDto> Categories,
  BookDetailDto? BookDetail
);

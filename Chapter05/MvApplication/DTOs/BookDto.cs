namespace MvApplication.DTOs;

public record BookDto(
  int Id,
  string Title,
  uint RowVersion,
  IReadOnlyCollection<CategoryDto> Categories,
  BookDetailDto? BookDetail
);

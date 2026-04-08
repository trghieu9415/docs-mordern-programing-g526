namespace MvApplication.DTOs;

public record UpdateBookRequest(
  string Title,
  string Summary,
  bool IsEbook,
  IReadOnlyCollection<int> CategoryIds,
  uint RowVersion
);

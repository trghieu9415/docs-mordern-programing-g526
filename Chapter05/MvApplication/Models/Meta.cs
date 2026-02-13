namespace MvApplication.Models;

public record Meta(int Page, int PageSize, int Total, int TotalPages) {
  public static Meta Create(int page, int pageSize, int total) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
    ArgumentOutOfRangeException.ThrowIfNegative(total);

    var totalPages = (total + pageSize - 1) / pageSize;
    return new Meta(page, pageSize, total, totalPages);
  }
}

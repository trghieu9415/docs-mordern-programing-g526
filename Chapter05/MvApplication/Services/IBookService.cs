using MvApplication.DTOs;

namespace MvApplication.Services;

public interface IBookService {
  Task<IReadOnlyCollection<BookDto>> GetListAsync(CancellationToken ct = default);
  Task<(BookDto Book, uint RowVersion)> GetDetailAsync(int id, CancellationToken ct = default);
  Task<(BookDto Book, uint RowVersion)> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct = default);
}

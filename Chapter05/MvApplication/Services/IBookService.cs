using MvApplication.DTOs;

namespace MvApplication.Services;

public interface IBookService {
  Task<IReadOnlyCollection<BookDto>> GetListAsync(CancellationToken ct = default);
  Task<BookDto> GetDetailAsync(int id, CancellationToken ct = default);
  Task<BookDto> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct = default);
}

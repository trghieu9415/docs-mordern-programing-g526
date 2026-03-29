using MvDomain.Entities;

namespace MvApplication.Repositories;

public interface ICategoryRepository {
  Task<IReadOnlyCollection<Category>> GetByIdsAsync(
    IReadOnlyCollection<int> categoryIds,
    CancellationToken ct = default
  );
}

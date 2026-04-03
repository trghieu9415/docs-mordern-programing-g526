using System.Linq.Expressions;
using MvDomain.Base;

namespace MvApplication.Repositories;

public interface IRepository<T> where T : BaseEntity {
  Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

  Task<T?> GetFirstAsync(
    Expression<Func<T, bool>>? criteria = null,
    CancellationToken ct = default
  );

  Task<List<T>> GetAsync(
    Expression<Func<T, bool>>? criteria = null,
    ICollection<Expression<Func<T, object>>>? includes = null,
    CancellationToken ct = default
  );

  Task<List<T>> GetByKeysAsync(
    ICollection<Guid> ids,
    ICollection<Expression<Func<T, object>>>? includes = null,
    CancellationToken ct = default
  );

  Task<IReadOnlyCollection<Guid>> GetMissingIdsAsync(ICollection<Guid> ids, CancellationToken ct = default);

  Task<Guid> CreateAsync(T entity, CancellationToken ct = default);
  Task UpdateAsync(T entity, CancellationToken ct = default);
  Task DeleteAsync(Guid id, CancellationToken ct = default);
}

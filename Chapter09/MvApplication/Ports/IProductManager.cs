using MvDomain.Entities;

namespace MvApplication.Ports;

public interface IProductManager {
  Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<(IList<Product> Products, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
  Task AddAsync(Product product, CancellationToken ct = default);
  Task UpdateAsync(Product product, CancellationToken ct = default);
  Task DeleteAsync(Guid id, CancellationToken ct = default);
}

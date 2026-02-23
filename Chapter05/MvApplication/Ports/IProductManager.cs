using MvDomain.Entities;

namespace MvApplication.Ports;

public interface IProductManager {
  // No-Tracking
  Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<(IList<Product> Products, int Total)> GetPagedAsync(
    int page, int pageSize, CancellationToken ct = default);

  //With-Tracking: cần cho Update/Delete 
  Task<Product?> GetByIdWithTrackingAsync(Guid id, CancellationToken ct = default);

  // Eager Loading: JOIN một lần, lấy tất cả relations
  Task<Product?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default);
  Task<(IList<Product> Products, int Total)> GetPagedWithCategoryAsync(
    int page, int pageSize, CancellationToken ct = default);

  // Lọc theo Category (Eager Loading + Where)
  Task<(IList<Product> Products, int Total)> GetPagedByCategoryAsync(
    int categoryId, int page, int pageSize, CancellationToken ct = default);
  
  /*
  Task LoadRelationsExplicitlyAsync(Product product, CancellationToken ct = default);
  */

  //  Pessimistic Locking: SELECT WITH (UPDLOCK, ROWLOCK)
  Task<Product?> GetByIdWithLockAsync(Guid id, CancellationToken ct = default);

  Task AddAsync(Product product, CancellationToken ct = default);
  void Update(Product product);  
  void Delete(Product product);  
}

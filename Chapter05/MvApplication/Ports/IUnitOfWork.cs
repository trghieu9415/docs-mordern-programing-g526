using Microsoft.EntityFrameworkCore.Storage;

namespace MvApplication.Ports;

public interface IUnitOfWork {
  IProductManager Products { get; }
  Task<int> SaveChangesAsync(CancellationToken ct = default);
  
  Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}

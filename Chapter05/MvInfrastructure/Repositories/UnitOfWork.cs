using MvApplication.Repositories;
using MvInfrastructure.Data;

namespace MvInfrastructure.Repositories;

public class UnitOfWork(LibraryDbContext context) : IUnitOfWork {
  public Task<int> SaveChangesAsync(CancellationToken ct = default) {
    return context.SaveChangesAsync(ct);
  }

  public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken ct = default) {
    var transaction = await context.Database.BeginTransactionAsync(ct);
    return new AppTransaction(transaction);
  }
}

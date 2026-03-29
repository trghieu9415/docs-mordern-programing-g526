using Microsoft.EntityFrameworkCore.Storage;
using MvApplication.Repositories;

namespace MvInfrastructure.Repositories;

public class AppTransaction(IDbContextTransaction transaction) : IAppTransaction {
  public Task CommitAsync(CancellationToken ct = default) {
    return transaction.CommitAsync(ct);
  }

  public Task RollbackAsync(CancellationToken ct = default) {
    return transaction.RollbackAsync(ct);
  }

  public ValueTask DisposeAsync() {
    return transaction.DisposeAsync();
  }
}

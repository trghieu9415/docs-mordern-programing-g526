namespace MvApplication.Repositories;

public interface IAppTransaction : IAsyncDisposable {
  Task CommitAsync(CancellationToken ct = default);
  Task RollbackAsync(CancellationToken ct = default);
}

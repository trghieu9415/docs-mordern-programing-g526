using System.Linq.Expressions;

namespace MvApplication.Ports;

public interface IBackgroundTaskQueue {
  public ValueTask QueueAsync<T>(Expression<Func<T, CancellationToken, Task>> workItem) where T : notnull;
  ValueTask<Func<CancellationToken, IServiceProvider, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

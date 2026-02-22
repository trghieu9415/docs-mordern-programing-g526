namespace MvApplication.Ports;

public interface IDistributedLockService {
  Task<IDisposable?> AcquireLockAsync(string resourceKey, TimeSpan wait);
}

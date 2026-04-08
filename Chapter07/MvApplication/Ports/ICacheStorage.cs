namespace MvApplication.Ports;

public interface ICacheStorage {
  Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
  Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
  Task RemoveAsync(string key, CancellationToken ct = default);
}

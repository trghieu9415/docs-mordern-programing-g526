using System.Text.Json;
using MvApplication.Ports;
using Microsoft.Extensions.Caching.Distributed;

namespace MvInfrastructure.Adapters;

public class RedisCacheStorage(IDistributedCache cache) : ICacheStorage {
  public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) {
    var cachedData = await cache.GetStringAsync(key, ct);
    return string.IsNullOrEmpty(cachedData) ? default : JsonSerializer.Deserialize<T>(cachedData);
  }

  public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) {
    var options = new DistributedCacheEntryOptions {
      AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
    };

    var jsonData = JsonSerializer.Serialize(value);
    await cache.SetStringAsync(key, jsonData, options, ct);
  }

  public async Task RemoveAsync(string key, CancellationToken ct = default) {
    await cache.RemoveAsync(key, ct);
  }
}

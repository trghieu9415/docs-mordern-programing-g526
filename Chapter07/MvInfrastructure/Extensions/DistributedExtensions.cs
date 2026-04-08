using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MvApplication.Ports;
using MvInfrastructure.Adapters;
using MvInfrastructure.Options;
using StackExchange.Redis;

namespace MvInfrastructure.Extensions;

public static class DistributedExtensions {
  /// <summary>Đăng ký Redis Cache và Distributed Lock. Cần gọi sau khi đã đăng ký <see cref="RedisOptions"/>.</summary>
  public static IServiceCollection AddDistributedInfrastructure(this IServiceCollection services) {
    // Redis Cache (IDistributedCache)
    services.AddOptions<RedisCacheOptions>()
      .Configure<IOptions<RedisOptions>>((cacheOpts, redisOpts) => {
        cacheOpts.Configuration = redisOpts.Value.Configuration;
        cacheOpts.InstanceName = redisOpts.Value.InstanceName;
      });
    services.AddStackExchangeRedisCache(_ => { });
    services.AddScoped<ICacheStorage, RedisCacheStorage>();

    // Redis Connection (cho Distributed Lock)
    services.AddSingleton<IConnectionMultiplexer>(sp => {
      var opts = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
      return ConnectionMultiplexer.Connect(opts.Configuration);
    });

    // Distributed Lock (Medallion + Redis)
    services.AddSingleton<IDistributedLockProvider>(sp => {
      var connection = sp.GetRequiredService<IConnectionMultiplexer>();
      return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
    });
    services.AddScoped<IDistributedLockService, RedisLockService>();

    return services;
  }
}

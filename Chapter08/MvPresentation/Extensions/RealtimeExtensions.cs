using MvApplication.Configs.Options;
using MvApplication.Ports;
using MvPresentation.Adapters;
using StackExchange.Redis;

namespace MvPresentation.Extensions;

public static class RealtimeExtensions {
  public static IServiceCollection AddSignalRAdapters(this IServiceCollection services, IConfiguration config) {
    var redisOptions = config.GetSection(RedisOptions.SectionName).Get<RedisOptions>()!;
    services
      .AddSignalR(options => {
        options.EnableDetailedErrors = true;
      })
      .AddStackExchangeRedis(redisOptions.Configuration, options => {
        options.Configuration.ChannelPrefix = RedisChannel.Literal(redisOptions.InstanceName);
      });
    ;
    services.AddScoped<IShowtimeNotifier, ShowtimeNotifier>();
    services.AddScoped<IUserNotifier, UserNotifier>();

    return services;
  }
}

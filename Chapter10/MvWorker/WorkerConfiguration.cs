using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MvApplication.Ports;
using MvWorker.Adapters;
using MvWorker.Extensions;

namespace Mv.Worker;

public static class WorkerConfiguration {
  public static IServiceCollection AddWorker(this IServiceCollection services, IConfiguration config) {
    services
      .AddCustomMassTransit(config)
      .AddFireAndForget();

    return services;
  }

  private static IServiceCollection AddFireAndForget(this IServiceCollection services) {
    services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(100));
    services.AddHostedService<QueuedHostedService>();
    return services;
  }
}

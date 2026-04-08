using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MvApplication.Configs.Options;
using MvApplication.Ports;
using MvInfrastructure.Persistence;
using MvWorker.Adapters;

namespace MvWorker.Extensions;

public static class MassTransitExtensions {
  public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration config) {
    services.AddMassTransit(x => {
      x.AddConsumers(typeof(WorkerConfiguration).Assembly);

      x.AddEntityFrameworkOutbox<AppDbContext>(o => {
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.UsePostgres();
        o.UseBusOutbox();
        o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
      });

      x.UsingRabbitMq((context, cfg) => {
        var options = config.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()!;

        cfg.Host(options.Host, options.VirtualHost, h => {
          h.Username(options.Username);
          h.Password(options.Password);
        });

        cfg.UseMessageRetry(r =>
          r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
        );
        cfg.UseCircuitBreaker(cb => {
          cb.TrackingPeriod = TimeSpan.FromMinutes(1);
          cb.TripThreshold = 15;
          cb.ActiveThreshold = 10;
          cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        cfg.ConfigureEndpoints(context);
      });
    });

    services.AddScoped<IEventDispatcher, MassTransitEventDispatcher>();
    return services;
  }
}

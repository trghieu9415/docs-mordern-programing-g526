using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MvInfrastructure.Extensions;

namespace MvInfrastructure;

public static class InfrastructureConfiguration {
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config) {
    services
      .AddConfigurationOptions(config)
      .AddMediatorPipeline()
      .AddExternalServices()
      .AddPostgresPersistence(config);
    return services;
  }
}

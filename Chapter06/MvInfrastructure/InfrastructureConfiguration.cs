using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MvApplication;
using MvApplication.Behaviors;
using MvApplication.Options;
using MvApplication.Ports;
using MvInfrastructure.Adapters;
using MvInfrastructure.Store;
using MvInfrastructure.Extensions;
using MvInfrastructure.Options;

namespace MvInfrastructure;

public static class InfrastructureConfiguration
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
  {
    services.AddApplication();

    // Product Options
    services.AddOptions<ProductOptions>()
      .Bind(config.GetSection(ProductOptions.SectionName))
      .ValidateDataAnnotations()
      .ValidateOnStart();

    services.AddSingleton(resolver =>
      resolver.GetRequiredService<IOptions<ProductOptions>>().Value);

    // JWT Options
    services.AddOptions<JwtOptions>()
      .Bind(config.GetSection(JwtOptions.SectionName))
      .ValidateDataAnnotations()
      .ValidateOnStart();

    services.AddSingleton(resolver =>
      resolver.GetRequiredService<IOptions<JwtOptions>>().Value);

    // Identity & Authentication
    services.AddIdentityInfrastructure(config);

    services.AddSingleton(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
    services.AddScoped<IProductManager, ProductManager>();

    services.AddSingleton<ProductStore>();

    return services;
  }

  private static IServiceCollection AddApplication(this IServiceCollection services)
  {
    var applicationAssembly = typeof(IApplicationMarker).Assembly;

    services.AddMediatR(cfg =>
    {
      cfg.RegisterServicesFromAssembly(applicationAssembly);
      cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    services.AddValidatorsFromAssembly(applicationAssembly);
    services.AddAutoMapper(_ => { }, applicationAssembly);

    services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
    return services;
  }
}

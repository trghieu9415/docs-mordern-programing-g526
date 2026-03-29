using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MvApplication;
using MvApplication.Behaviors;
using MvApplication.Configs;
using MvApplication.Configs.Options;
using MvApplication.Ports;
using MvInfrastructure.Adapters;

namespace MvInfrastructure;

public static class InfrastructureConfiguration {
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config) {
    services.AddApplication();

    services.AddSingleton<ICinemaDataStore, CinemaDataStore>();
    services.AddSingleton<ISeatStateStore, SeatStateStore>();
    services.AddSingleton<IUserDataStore, UserDataStore>();

    services.AddScoped<IBookingService, BookingService>();
    services.AddScoped<IAuthService, AuthService>();

    services.AddTransient<ITicketPriceCalculator, TicketPriceCalculator>();
    services.AddTransient<IJwtService, JwtService>();

    services.RegisterOption<CinemaSettings>(config);
    services.RegisterOption<JwtSettings>(config);

    return services;
  }

  private static IServiceCollection AddApplication(this IServiceCollection services) {
    var applicationAssembly = typeof(IApplicationMarker).Assembly;

    services.AddMediatR(cfg => {
      cfg.RegisterServicesFromAssembly(applicationAssembly);
      cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    services.AddValidatorsFromAssembly(applicationAssembly);
    return services;
  }

  private static void RegisterOption<TOptions>(this IServiceCollection services, IConfiguration config)
    where TOptions : class, IOptionSection {
    var sectionName = typeof(TOptions).GetProperty("SectionName")?.GetValue(null) as string;

    services.AddOptions<TOptions>()
      .Bind(config.GetSection(sectionName!))
      .ValidateDataAnnotations()
      .ValidateOnStart();

    services.AddSingleton(resolver =>
      resolver.GetRequiredService<IOptions<TOptions>>().Value);
  }
}

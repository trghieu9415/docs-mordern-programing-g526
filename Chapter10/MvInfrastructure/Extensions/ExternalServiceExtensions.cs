using Microsoft.Extensions.DependencyInjection;
using MvApplication.Ports;
using MvInfrastructure.Adapters;

namespace MvInfrastructure.Extensions;

public static class ExternalServiceExtensions {
  public static IServiceCollection AddExternalServices(this IServiceCollection services) {
    services.AddScoped<IEmailService, ConsoleEmailService>();

    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IJwtService, JwtService>();
    return services;
  }
}

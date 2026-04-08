using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MvApplication.Configs.Options;
using MvApplication.Ports;
using MvInfrastructure.Adapters;
using MvPresentation.Adapters;

namespace MvPresentation.Extensions;

public static class SecurityExtensions {
  public static IServiceCollection AddJwtAuthentication(
    this IServiceCollection services,
    IConfiguration config
  ) {
    services.AddScoped<ICurrentUser, CurrentUser>();

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => {
        var jwtSettings = config.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
        options.TokenValidationParameters = JwtService.GetTokenValidationParameters(jwtSettings);
        options.Events = GetEvents();
      });

    return services;
  }

  private static JwtBearerEvents GetEvents() {
    return new JwtBearerEvents {
      OnTokenValidated = context => {
        var claims = context.Principal;

        var userId = claims?.FindFirstValue(ClaimTypes.NameIdentifier);

        if ( 
          string.IsNullOrEmpty(userId) ||
          !Guid.TryParse(userId, out _)
        ) {
          context.Fail("Unauthorized");
        }

        return Task.CompletedTask;
      },
      OnMessageReceived = context => {
        var accessToken = context.Request.Query["access_token"];
        if (
          !string.IsNullOrEmpty(accessToken) &&
          context.HttpContext.Request.Path.StartsWithSegments("/hubs")
        ) {
          context.Token = accessToken;
        }

        return Task.CompletedTask;
      }
    };
  }
}

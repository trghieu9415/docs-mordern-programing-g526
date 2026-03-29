using System.Text.Json;
using System.Text.Json.Serialization;

namespace MvPresentation.Extensions;

public static class ApiBehaviorExtensions {
  public static IServiceCollection AddWebApiDefaults(this IServiceCollection services) {
    // --- Controller & JSON Config ---
    services
      .AddControllers()
      .ConfigureApiBehaviorOptions(options => {
        options.SuppressModelStateInvalidFilter = true;
      })
      .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
      });

    // --- Routing Configuration ---
    services.AddRouting(options => {
      options.LowercaseUrls = true;
      options.LowercaseQueryStrings = true;
    });

    return services;
  }
}

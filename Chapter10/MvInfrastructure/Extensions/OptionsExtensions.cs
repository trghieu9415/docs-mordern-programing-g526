using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MvApplication.Configs;
using MvApplication.Configs.Options;

namespace MvInfrastructure.Extensions;

public static class OptionsExtensions {
  public static IServiceCollection AddConfigurationOptions(
    this IServiceCollection services,
    IConfiguration config
  ) {
    services.RegisterOption<JwtSettings>(config);
    services.RegisterOption<RabbitMqOptions>(config);

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

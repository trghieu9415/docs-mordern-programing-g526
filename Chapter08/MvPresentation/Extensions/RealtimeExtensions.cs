using MvApplication.Ports;
using MvPresentation.Adapters;

namespace MvPresentation.Extensions;

public static class RealtimeExtensions {
  public static IServiceCollection AddSignalRAdapters(this IServiceCollection services) {
    services.AddSignalR(options => {
      options.EnableDetailedErrors = true;
    });
    services.AddScoped<IShowtimeNotifier, ShowtimeNotifier>();
    services.AddScoped<IUserNotifier, UserNotifier>();

    return services;
  }
}

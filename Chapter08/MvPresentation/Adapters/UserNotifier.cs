using Microsoft.AspNetCore.SignalR;
using MvApplication.Ports;
using MvPresentation.Hubs;

namespace MvPresentation.Adapters;

public class UserNotifier(IHubContext<CinemaHub> hubContext) : IUserNotifier {
  public async Task SendPersonalMessageAsync(string userId, string message, CancellationToken ct = default) {
    await hubContext.Clients.User(userId)
      .SendAsync("ReceiveNotification", message, ct);
  }
}

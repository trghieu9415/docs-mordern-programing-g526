using Microsoft.AspNetCore.SignalR;
using MvApplication.Ports;
using MvPresentation.Hubs;

namespace MvPresentation.Adapters;

public class ShowtimeNotifier(
  IHubContext<CinemaHub> hubContext
) : IShowtimeNotifier {
  public async Task NotifySeatReleasedAsync(
    Guid showtimeId,
    IEnumerable<string> seatCodes,
    CancellationToken ct = default
  ) {
    await hubContext.Clients.Group(showtimeId.ToString())
      .SendAsync("SeatReleased", seatCodes, ct);
  }

  public async Task NotifySeaHeldAsync(
    Guid showtimeId,
    string seatCode,
    CancellationToken ct = default
  ) {
    await hubContext.Clients.Group(showtimeId.ToString())
      .SendAsync("SeatHeld", seatCode, ct);
  }

  public async Task NotifySeatSoldAsync(
    Guid showtimeId, IEnumerable<string> seatCodes,
    CancellationToken ct = default
  ) {
    await hubContext.Clients.Group(showtimeId.ToString())
      .SendAsync("SeatSold", seatCodes, ct);
  }
}

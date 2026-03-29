using System.Collections.Concurrent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MvApplication.Ports;
using MvApplication.UseCases.HoldSeat;
using MvApplication.UseCases.ReleaseAllSeats;
using MvApplication.UseCases.ReleaseSeat;

namespace MvPresentation.Hubs;

[Authorize]
public class CinemaHub(
  IMediator mediator,
  ISeatStateStore seatStateStore
) : Hub {
  private static int _userCount;

  private static readonly ConcurrentDictionary<string, Guid> Sessions = new();

  public async Task JoinShowtimeGroup(Guid showtimeId) {
    var showtime = showtimeId.ToString();
    await Groups.AddToGroupAsync(Context.ConnectionId, showtime);
    Sessions[Context.ConnectionId] = showtimeId;
    var seatInfo = new SeatInfo {
      BookedSeats = seatStateStore.GetAllBookedSeats(showtimeId),
      HeldSeats = seatStateStore.GetAllHeldSeats(showtimeId)
    };
    await Clients.Caller.SendAsync("SeatStatuses", seatInfo);
    await Clients.Group(showtime).SendAsync("NewUserJoined", ++_userCount);
  }

  public override async Task OnDisconnectedAsync(Exception? exception) {
    _ = Guid.TryParse(Context.UserIdentifier, out var userId);
    var showtimeId = Sessions[Context.ConnectionId];
    await mediator.Send(new ReleaseAllSeatsCommand(showtimeId, userId));
    await Clients.OthersInGroup(showtimeId.ToString()).SendAsync("NewUserOut", --_userCount);
    await base.OnDisconnectedAsync(exception);
  }

  public async Task HoldSeat(string seatCode) {
    var ct = Context.ConnectionAborted;
    _ = Guid.TryParse(Context.UserIdentifier, out var userId);
    var showtimeId = Sessions[Context.ConnectionId];
    await mediator.Send(new HoldSeatCommand(showtimeId, userId, seatCode), ct);
  }

  public async Task ReleaseSeat(string seatCode) {
    var ct = Context.ConnectionAborted;
    _ = Guid.TryParse(Context.UserIdentifier, out var userId);
    var showtimeId = Sessions[Context.ConnectionId];
    await mediator.Send(new ReleaseSeatCommand(showtimeId, userId, seatCode), ct);
  }
}

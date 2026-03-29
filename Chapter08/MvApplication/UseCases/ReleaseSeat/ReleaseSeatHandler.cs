using MediatR;
using MvApplication.Ports;

namespace MvApplication.UseCases.ReleaseSeat;

public class ReleaseSeatHandler(
  ISeatStateStore seatStateStore,
  IShowtimeNotifier showtimeNotifier
) : IRequestHandler<ReleaseSeatCommand, ReleaseSeatsResult> {
  public async Task<ReleaseSeatsResult> Handle(ReleaseSeatCommand request, CancellationToken ct) {
    seatStateStore.ReleaseSeat(request.ShowtimeId, request.UserId, request.SeatCode);
    await showtimeNotifier.NotifySeatReleasedAsync(request.ShowtimeId, [request.SeatCode], ct);
    return new ReleaseSeatsResult(true);
  }
}

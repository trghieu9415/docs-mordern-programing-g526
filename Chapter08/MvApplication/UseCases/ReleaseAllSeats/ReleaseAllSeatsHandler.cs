using MediatR;
using MvApplication.Ports;

namespace MvApplication.UseCases.ReleaseAllSeats;

public class ReleaseAllSeatsHandler(
  ISeatStateStore seatStateStore,
  IShowtimeNotifier showtimeNotifier
) : IRequestHandler<ReleaseAllSeatsCommand, ReleaseAllSeatsResult> {
  public async Task<ReleaseAllSeatsResult> Handle(ReleaseAllSeatsCommand request, CancellationToken ct) {
    var releasedSeats = seatStateStore.ReleaseAllHeldSeats(request.ShowtimeId, request.UserId);
    if (releasedSeats.Count != 0) {
      await showtimeNotifier.NotifySeatReleasedAsync(request.ShowtimeId, releasedSeats, ct);
    }

    return new ReleaseAllSeatsResult(releasedSeats);
  }
}

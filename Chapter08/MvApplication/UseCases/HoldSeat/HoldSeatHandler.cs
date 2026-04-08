using MediatR;
using MvApplication.Ports;

namespace MvApplication.UseCases.HoldSeat;

public class HoldSeatHandler(
  ISeatStateStore seatStateStore,
  IShowtimeNotifier showtimeNotifier
) : IRequestHandler<HoldSeatCommand, HoldSeatsResult> {
  public async Task<HoldSeatsResult> Handle(HoldSeatCommand request, CancellationToken ct) {
    seatStateStore.HoldSeat(request.ShowtimeId, request.UserId, request.SeatCode);
    await showtimeNotifier.NotifySeaHeldAsync(request.ShowtimeId, request.SeatCode, ct);
    return new HoldSeatsResult(true);
  }
}

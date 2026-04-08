namespace MvApplication.Ports;

public interface IShowtimeNotifier {
  Task NotifySeatReleasedAsync(Guid showtimeId, IEnumerable<string> seatCodes, CancellationToken ct = default);
  Task NotifySeaHeldAsync(Guid showtimeId, string seatCodes, CancellationToken ct = default);
  Task NotifySeatSoldAsync(Guid showtimeId, IEnumerable<string> seatCodes, CancellationToken ct = default);
}

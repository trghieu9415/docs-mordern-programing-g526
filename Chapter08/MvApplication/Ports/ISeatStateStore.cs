namespace MvApplication.Ports;

public interface ISeatStateStore {
  void HoldSeat(Guid showtimeId, Guid userId, string seatCode);
  void ReleaseSeat(Guid showtimeId, Guid userId, string seatCode);
  void BookSeats(Guid showtimeId, Guid userId, List<string> seatCodes);

  SeatInfo? GetUserSeats(Guid showtimeId, Guid userId);
  List<string> GetAllHeldSeats(Guid showtimeId);
  List<string> GetAllBookedSeats(Guid showtimeId);
  List<string> ReleaseAllHeldSeats(Guid showtimeId, Guid userId);
}

public class SeatInfo {
  public List<string> BookedSeats { get; set; } = [];
  public List<string> HeldSeats { get; set; } = [];
}

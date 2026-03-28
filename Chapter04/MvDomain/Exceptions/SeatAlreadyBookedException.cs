namespace MvDomain.Exceptions;

public class SeatAlreadyBookedException(IEnumerable<string> seatCodes) : DomainException("Ghế được chọn đã được đặt!") {
  public IEnumerable<string> ConflictingSeats { get; private set; } = seatCodes;
}

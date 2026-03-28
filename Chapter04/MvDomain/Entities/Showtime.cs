using MvDomain.Base;

namespace MvDomain.Entities;

public class Showtime : BaseEntity {
  private readonly List<string> _bookedSeats = [];
  private Showtime() {}
  public string MovieTitle { get; private set; } = null!;
  public DateTime StartTime { get; private set; }
  public IReadOnlyCollection<string> BookedSeats => _bookedSeats.AsReadOnly();

  public static Showtime Create(string movieTitle, DateTime startTime) {
    return new Showtime {
      MovieTitle = movieTitle,
      StartTime = startTime
    };
  }

  public void BookSeats(IEnumerable<string> seatCodes) {
    _bookedSeats.AddRange(seatCodes);
  }
}

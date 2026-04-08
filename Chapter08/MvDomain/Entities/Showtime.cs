using MvDomain.Base;

namespace MvDomain.Entities;

public class Showtime : BaseEntity {
  private readonly Dictionary<string, List<string>> _userSeats = [];
  private Showtime() {}
  public string MovieTitle { get; private set; } = null!;
  public DateTime StartTime { get; private set; }
  public IReadOnlyDictionary<string, List<string>> UserSeats => _userSeats;

  public IReadOnlyCollection<string> BookedSeats =>
    _userSeats.Values.SelectMany(seats => seats).ToList().AsReadOnly();

  public static Showtime Create(string movieTitle, DateTime startTime) {
    return new Showtime {
      MovieTitle = movieTitle,
      StartTime = startTime
    };
  }

  public void BookSeats(string userId, IEnumerable<string> seatCodes) {
    var seats = seatCodes.ToList();
    if (!_userSeats.TryGetValue(userId, out var value)) {
      value = [];
      _userSeats[userId] = value;
    }

    value.AddRange(seats);
  }

  public IReadOnlyCollection<string> GetBookedSeatsByUser(string userId) {
    if (_userSeats.TryGetValue(userId, out var seats)) {
      return seats.AsReadOnly();
    }

    return [];
  }
}

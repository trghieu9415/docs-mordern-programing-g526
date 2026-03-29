using System.Collections.Concurrent;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvInfrastructure.Adapters;

public class SeatStateStore : ISeatStateStore {
  private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, SeatInfo>> _showtimeSeats = new();

  public void HoldSeat(Guid showtimeId, Guid userId, string seatCode) {
    var userSeats = _showtimeSeats.GetOrAdd(
      showtimeId, _ => new ConcurrentDictionary<Guid, SeatInfo>()
    );

    foreach (var (currentUserId, info) in userSeats) {
      lock (info) {
        if (info.BookedSeats.Contains(seatCode)) {
          throw new WorkflowException("Ghế này đã được bán.");
        }

        if (currentUserId != userId && info.HeldSeats.Contains(seatCode)) {
          throw new WorkflowException("Ghế này đang được người khác giữ.");
        }
      }
    }

    var seatInfo = userSeats.GetOrAdd(userId, _ => new SeatInfo());
    lock (seatInfo) {
      if (!seatInfo.HeldSeats.Contains(seatCode)) {
        seatInfo.HeldSeats.Add(seatCode);
      }
    }
  }

  public void ReleaseSeat(Guid showtimeId, Guid userId, string seatCode) {
    if (!_showtimeSeats.TryGetValue(showtimeId, out var userSeats) ||
        !userSeats.TryGetValue(userId, out var seatInfo)) {
      return;
    }

    lock (seatInfo) {
      seatInfo.HeldSeats.Remove(seatCode);
    }
  }

  public void BookSeats(Guid showtimeId, Guid userId, List<string> seatCodes) {
    var userSeats = _showtimeSeats.GetOrAdd(
      showtimeId, _ => new ConcurrentDictionary<Guid, SeatInfo>()
    );
    var seatInfo = userSeats.GetOrAdd(userId, _ => new SeatInfo());

    lock (seatInfo) {
      foreach (var seat in seatCodes) {
        seatInfo.HeldSeats.Remove(seat);
      }

      seatInfo.BookedSeats.AddRange(seatCodes);
    }
  }

  public SeatInfo? GetUserSeats(Guid showtimeId, Guid userId) {
    if (!_showtimeSeats.TryGetValue(showtimeId, out var userSeats) ||
        !userSeats.TryGetValue(userId, out var seatInfo)) {
      return null;
    }

    lock (seatInfo) {
      return new SeatInfo {
        BookedSeats = [..seatInfo.BookedSeats],
        HeldSeats = [..seatInfo.HeldSeats]
      };
    }
  }

  public List<string> GetAllHeldSeats(Guid showtimeId) {
    if (!_showtimeSeats.TryGetValue(showtimeId, out var userSeats)) {
      return [];
    }

    var allSeats = new List<string>();
    foreach (var seatInfo in userSeats.Values) {
      lock (seatInfo) {
        allSeats.AddRange(seatInfo.HeldSeats);
      }
    }

    return allSeats;
  }

  public List<string> GetAllBookedSeats(Guid showtimeId) {
    if (!_showtimeSeats.TryGetValue(showtimeId, out var userSeats)) {
      return [];
    }

    var allSeats = new List<string>();
    foreach (var seatInfo in userSeats.Values) {
      lock (seatInfo) {
        allSeats.AddRange(seatInfo.BookedSeats);
      }
    }

    return allSeats;
  }

  public List<string> ReleaseAllHeldSeats(Guid showtimeId, Guid userId) {
    if (!_showtimeSeats.TryGetValue(showtimeId, out var userSeats) ||
        !userSeats.TryGetValue(userId, out var seatInfo)) {
      return [];
    }

    lock (seatInfo) {
      var releasedSeats = new List<string>(seatInfo.HeldSeats);
      seatInfo.HeldSeats.Clear();
      return releasedSeats;
    }
  }
}

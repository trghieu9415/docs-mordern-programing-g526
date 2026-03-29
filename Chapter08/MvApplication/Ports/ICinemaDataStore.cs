using MvDomain.Entities;

namespace MvApplication.Ports;

public interface ICinemaDataStore {
  Task<Showtime?> GetShowtimeByIdAsync(Guid id, CancellationToken ct);
  Task<List<Showtime>> GetAllShowtimesAsync(CancellationToken ct);
  Task UpdateShowtimeAsync(Showtime showtime, CancellationToken ct);
}

using MvApplication.Ports;
using MvDomain.Entities;

namespace MvInfrastructure.Adapters;

public class CinemaDataStore : ICinemaDataStore {
  private readonly List<Showtime> _showtimes = [];

  public CinemaDataStore() {
    _showtimes.Add(Showtime.Create("Wandering Sword", new DateTime(2026, 1, 1, 7, 0, 0)));
    _showtimes.Add(Showtime.Create("Batman 2", new DateTime(2026, 1, 1, 10, 0, 0)));
  }

  public async Task<Showtime?> GetShowtimeByIdAsync(Guid id, CancellationToken ct) {
    await Task.Delay(500, ct);
    return _showtimes.FirstOrDefault(s => s.Id == id);
  }

  public async Task<List<Showtime>> GetAllShowtimesAsync(CancellationToken ct) {
    return await Task.FromResult(_showtimes);
  }

  public async Task UpdateShowtimeAsync(Showtime showtime, CancellationToken ct) {
    await Task.Delay(100, ct);
  }
}

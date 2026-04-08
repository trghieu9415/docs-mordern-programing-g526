using MvApplication.Ports;
using MvDomain.Entities;

namespace MvInfrastructure.Adapters;

public class CinemaDataStore : ICinemaDataStore {
  private readonly List<Showtime> _showtimes = [];

  public CinemaDataStore() {
    _showtimes.Add(Showtime.Create("Wandering Sword", DateTime.Now.AddDays(1)));
  }

  public async Task<Showtime?> GetShowtimeByIdAsync(Guid id, CancellationToken ct) {
    await Task.Delay(2000, ct);
    return _showtimes.FirstOrDefault(s => s.Id == id);
  }

  public async Task<List<Showtime>> GetAllShowtimesAsync(CancellationToken ct) {
    return await Task.FromResult(_showtimes);
  }

  public async Task UpdateShowtimeAsync(Showtime showtime, CancellationToken ct) {
    await Task.Delay(100, ct);
  }
}

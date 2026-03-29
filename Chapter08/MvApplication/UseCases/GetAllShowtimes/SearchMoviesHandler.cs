using MediatR;
using MvApplication.DTOs;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetAllShowtimes;

public class GetAllShowtimesHandler(
  ICinemaDataStore store
) : IRequestHandler<GetAllShowtimesQuery, GetAllShowtimesResult> {
  public async Task<GetAllShowtimesResult> Handle(GetAllShowtimesQuery request, CancellationToken ct) {
    var all = await store.GetAllShowtimesAsync(ct);
    var showtimes = all
      .Select(s => new ShowtimeDto(s.Id, s.MovieTitle, s.StartTime))
      .ToList();
    return new GetAllShowtimesResult(showtimes);
  }
}

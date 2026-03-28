using MediatR;
using MvApplication.DTOs;
using MvApplication.Ports;

namespace MvApplication.UseCases.SearchMovies;

public class SearchMoviesHandler(
  ICinemaDataStore store
) : IRequestHandler<SearchMoviesQuery, SearchMoviesResult> {
  public async Task<SearchMoviesResult> Handle(SearchMoviesQuery request, CancellationToken ct) {
    var all = await store.GetAllShowtimesAsync(ct);
    var showtimes = all
      .Where(s =>
        string.IsNullOrEmpty(request.SearchKey) ||
        s.MovieTitle.Contains(request.SearchKey, StringComparison.CurrentCultureIgnoreCase))
      .Skip((request.Page - 1) * request.Size)
      .Take(request.Size)
      .Select(s => new ShowtimeDto(s.Id, s.MovieTitle, s.StartTime))
      .ToList();
    return new SearchMoviesResult(showtimes);
  }
}

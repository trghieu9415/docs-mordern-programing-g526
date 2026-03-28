using MediatR;
using MvApplication.DTOs;

namespace MvApplication.UseCases.SearchMovies;

public record SearchMoviesQuery : IRequest<SearchMoviesResult> {
  public int Page { get; init; } = 1;
  public int Size { get; init; } = 10;
  public string? SearchKey { get; init; }
}

public record SearchMoviesResult(
  List<ShowtimeDto> Showtimes
);

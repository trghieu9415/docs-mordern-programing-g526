using MediatR;
using MvApplication.DTOs;

namespace MvApplication.UseCases.GetAllShowtimes;

public record GetAllShowtimesQuery : IRequest<GetAllShowtimesResult>;

public record GetAllShowtimesResult(
  List<ShowtimeDto> Showtimes
);

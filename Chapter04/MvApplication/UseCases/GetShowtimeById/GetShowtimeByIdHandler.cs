using MediatR;
using MvApplication.DTOs;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetShowtimeById;

public class GetShowtimeByIdHandler(
  ICinemaDataStore store
) : IRequestHandler<GetShowtimeByIdQuery, GetShowtimeByIdResult> {
  public async Task<GetShowtimeByIdResult> Handle(GetShowtimeByIdQuery request, CancellationToken ct) {
    var showtime = await store.GetShowtimeByIdAsync(request.Id, ct);
    if (showtime == null) {
      throw new KeyNotFoundException("Không thấy lịch này!");
    }

    return new GetShowtimeByIdResult(
      new ShowtimeDto(showtime.Id, showtime.MovieTitle, showtime.StartTime)
    );
  }
}

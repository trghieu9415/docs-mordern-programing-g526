using MediatR;
using MvApplication.DTOs;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.GetShowtimeById;

public class GetShowtimeByIdHandler(
  ICinemaDataStore store,
  IBookingService bookingService
) : IRequestHandler<GetShowtimeByIdQuery, GetShowtimeByIdResult> {
  public async Task<GetShowtimeByIdResult> Handle(GetShowtimeByIdQuery request, CancellationToken ct) {
    var showtimeTask = store.GetShowtimeByIdAsync(request.Id, ct);
    var bookedSeatsTask = bookingService.GetBookedSeatsAsync(request.Id, ct);

    await Task.WhenAll(showtimeTask, bookedSeatsTask);

    var showtime = await showtimeTask ?? throw new WorkFlowException("Không thấy showtime", 404);
    var bookedSeats = await bookedSeatsTask;

    return new GetShowtimeByIdResult(
      new ShowtimeDto(showtime.Id, showtime.MovieTitle, showtime.StartTime, bookedSeats)
    );
  }
}

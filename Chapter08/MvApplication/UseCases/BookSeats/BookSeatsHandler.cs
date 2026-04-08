using MediatR;
using MvApplication.DTOs;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.BookSeats;

public class BookSeatsHandler(
  IBookingService bookingService,
  ISeatStateStore seatStateStore,
  IShowtimeNotifier showtimeNotifier
) : IRequestHandler<BookSeatsCommand, BookSeatsResult> {
  public async Task<BookSeatsResult> Handle(BookSeatsCommand request, CancellationToken ct) {
    var heldSeats = seatStateStore.GetUserSeats(request.ShowtimeId, request.UserId);
    if (heldSeats is null || heldSeats.HeldSeats.Count == 0) {
      throw new WorkflowException("Không có ghế chọn để thanh toán");
    }
    
    var ticket = await bookingService.BookTicketAsync(
      request.ShowtimeId, request.UserId, heldSeats.HeldSeats,
      ct
    );

    await showtimeNotifier.NotifySeatSoldAsync(request.ShowtimeId, ticket.SeatCodes, ct);
    return new BookSeatsResult(new TicketDto(ticket.ShowtimeId, ticket.SeatCodes, ticket.TotalPrice));
  }
}

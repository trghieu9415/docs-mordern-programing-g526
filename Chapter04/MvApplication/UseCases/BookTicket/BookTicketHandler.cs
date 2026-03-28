using MediatR;
using MvApplication.DTOs;
using MvApplication.Ports;

namespace MvApplication.UseCases.BookTicket;

public class BookTicketHandler(IBookingService service) : IRequestHandler<BookTicketCommand, BookTicketResult> {
  public async Task<BookTicketResult> Handle(BookTicketCommand request, CancellationToken ct) {
    var ticket = await service.BookTicketAsync(request.ShowtimeId, request.SeatCodes, ct);
    return new BookTicketResult(new TicketDto(ticket.ShowtimeId, ticket.SeatCodes, ticket.TotalPrice));
  }
}

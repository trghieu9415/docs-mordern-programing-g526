using MvDomain.Entities;

namespace MvApplication.Ports;

public interface IBookingService {
  Task<Ticket> BookTicketAsync(Guid showtimeId, List<string> seatCodes, CancellationToken ct);
}

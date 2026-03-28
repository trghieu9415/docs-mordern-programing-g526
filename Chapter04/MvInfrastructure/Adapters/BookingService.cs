using MvApplication.Configs.Options;
using MvApplication.Ports;
using MvDomain.Entities;
using MvDomain.Exceptions;

namespace MvInfrastructure.Adapters;

public class BookingService(
  ICinemaDataStore dataStore,
  ITicketPriceCalculator calculator,
  CinemaSettings settings
) : IBookingService {
  public async Task<Ticket> BookTicketAsync(Guid showtimeId, List<string> seatCodes, CancellationToken ct) {
    if (seatCodes.Count > settings.MaxTicketsPerBooking) {
      throw new ArgumentException($"Chỉ được đặt tối đa {settings.MaxTicketsPerBooking} vé!");
    }

    var showtime =
      await dataStore.GetShowtimeByIdAsync(showtimeId, ct)
      ?? throw new KeyNotFoundException("Không tìm thấy lịch chiếu này.");

    var alreadyBooked = seatCodes.Intersect(showtime.BookedSeats).ToList();
    if (alreadyBooked.Count != 0) {
      throw new SeatAlreadyBookedException(alreadyBooked);
    }

    await Task.Delay(1000, ct);
    var totalPrice = calculator.Calculate(showtime.StartTime) * seatCodes.Count;

    showtime.BookSeats(seatCodes);
    await dataStore.UpdateShowtimeAsync(showtime, ct);

    return Ticket.Create(showtimeId, seatCodes, totalPrice);
  }
}

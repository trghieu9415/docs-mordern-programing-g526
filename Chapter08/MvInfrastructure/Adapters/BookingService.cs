using MvApplication.Configs.Options;
using MvApplication.Ports;
using MvDomain.Entities;
using MvDomain.Exceptions;

namespace MvInfrastructure.Adapters;

public class BookingService(
  ICinemaDataStore dataStore,
  ITicketPriceCalculator calculator,
  ISeatStateStore seatStateStore,
  CinemaSettings settings
) : IBookingService {
  public async Task<List<string>>
    GetBookedSeatsAsync(Guid showtimeId, CancellationToken ct = default) {
    var showtime =
      await dataStore.GetShowtimeByIdAsync(showtimeId, ct)
      ?? throw new KeyNotFoundException("Không tìm thấy lịch chiếu này.");

    return showtime.BookedSeats.ToList();
  }

  public async Task<Ticket>
    BookTicketAsync(Guid showtimeId, Guid userId, List<string> seatCodes, CancellationToken ct) {
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

    var totalPrice = calculator.Calculate(showtime.StartTime) * seatCodes.Count;
    showtime.BookSeats(userId.ToString(), seatCodes);
    await dataStore.UpdateShowtimeAsync(showtime, ct);

    seatStateStore.BookSeats(showtimeId, userId, seatCodes);
    return Ticket.Create(showtimeId, seatCodes, totalPrice);
  }
}

namespace MvApplication.DTOs;

public record EventDto(
  Guid Id,
  string Name,
  string Description,
  string Venue,
  DateTime StartAt,
  decimal TicketPrice,
  int Capacity,
  int AvailableTickets
);

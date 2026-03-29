using MediatR;

namespace MvApplication.UseCases.Events.CreateEvent;

public record CreateEventCommand(
  string Name,
  string Description,
  string Venue,
  DateTime StartAt,
  decimal TicketPrice,
  int Capacity
) : IRequest<Guid>;

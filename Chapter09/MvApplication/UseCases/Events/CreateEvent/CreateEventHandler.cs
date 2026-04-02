using MediatR;
using MvApplication.Ports;
using MvDomain.Entities;

namespace MvApplication.UseCases.Events.CreateEvent;

public class CreateEventHandler(IEventRepository repository) : IRequestHandler<CreateEventCommand, Guid> {
  public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken) {
    var entity = Event.Create(
      request.Name,
      request.Description,
      request.Venue,
      request.StartAt,
      request.TicketPrice,
      request.Capacity);

    await repository.AddAsync(entity, cancellationToken);
    return entity.Id;
  }
}

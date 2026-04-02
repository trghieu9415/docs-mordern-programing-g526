using AutoMapper;
using MediatR;
using MvApplication.DTOs;
using MvApplication.Ports;

namespace MvApplication.UseCases.Events.GetEvents;

public class GetEventsHandler(IEventRepository repository, IMapper mapper) : IRequestHandler<GetEventsQuery, IList<EventDto>> {
  public async Task<IList<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken) {
    var entities = await repository.GetAllAsync(cancellationToken);
    return mapper.Map<IList<EventDto>>(entities);
  }
}

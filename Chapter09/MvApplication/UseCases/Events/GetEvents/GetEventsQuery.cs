using MediatR;
using MvApplication.DTOs;

namespace MvApplication.UseCases.Events.GetEvents;

public record GetEventsQuery() : IRequest<IList<EventDto>>;

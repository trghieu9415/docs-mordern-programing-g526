using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.Events.CreateEvent;
using MvApplication.UseCases.Events.GetEvents;
using MvPresentation.Response;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/events")]
[ApiExplorerSettings(GroupName = "v1")]
public class EventsController(IMediator mediator) : ControllerBase {
  [HttpGet]
  public async Task<IActionResult> GetEvents(CancellationToken ct) {
    var result = await mediator.Send(new GetEventsQuery(), ct);
    return AppResponse.Success(result);
  }

  [HttpPost]
  public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command, CancellationToken ct) {
    var eventId = await mediator.Send(command, ct);
    return AppResponse.Success(eventId, "Tao su kien thanh cong", 201);
  }
}

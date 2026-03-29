using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.Events.CreateEvent;
using MvApplication.UseCases.Events.GetEvents;
using MvApplication.UseCases.Events.UploadPoster;
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

  [HttpPost("{eventId:guid}/poster")]
  [RequestSizeLimit(10_000_000)]
  public async Task<IActionResult> UploadPoster(Guid eventId, IFormFile file, CancellationToken ct) {
    if (file is null || file.Length == 0) {
      return AppResponse.Fail("File poster khong hop le.", 400);
    }

    await using var stream = file.OpenReadStream();
    var posterUrl = await mediator.Send(new UploadPosterCommand(eventId, stream, file.FileName, file.ContentType), ct);
    return AppResponse.Success(new { EventId = eventId, PosterUrl = posterUrl }, "Upload poster thanh cong");
  }
}

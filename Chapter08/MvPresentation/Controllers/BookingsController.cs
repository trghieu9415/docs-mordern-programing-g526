using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvApplication.Ports;
using MvApplication.UseCases.BookSeats;
using MvPresentation.Filters;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
public class BookingsController(
  IMediator mediator,
  ICurrentUser user
) : ControllerBase {
  [HttpPost("book")]
  [ServiceFilter(typeof(PerformanceMonitorFilter))]
  public async Task<IActionResult> Book([FromBody] Guid showtimeId, CancellationToken ct) {
    var command = new BookSeatsCommand(showtimeId, user.Id);
    var result = await mediator.Send(command, ct);
    return Ok(result);
  }
}

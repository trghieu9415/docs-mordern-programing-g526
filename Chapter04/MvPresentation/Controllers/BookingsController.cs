using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.BookTicket;
using MvPresentation.Filters;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class BookingsController(IMediator mediator) : ControllerBase {
  [HttpPost("book")]
  [ServiceFilter(typeof(PerformanceMonitorFilter))]
  public async Task<IActionResult> Book([FromBody] BookTicketCommand command, CancellationToken ct) {
    var result = await mediator.Send(command, ct);
    return Ok(result);
  }
}

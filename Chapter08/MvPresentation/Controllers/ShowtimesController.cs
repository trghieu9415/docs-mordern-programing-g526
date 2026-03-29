using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.GetAllShowtimes;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class ShowtimesController(IMediator mediator) : ControllerBase {
  [HttpGet]
  public async Task<IActionResult> GetAll(CancellationToken ct) {
    var result = await mediator.Send(new GetAllShowtimesQuery(), ct);
    return Ok(result);
  }
}

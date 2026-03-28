using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.GetShowtimeById;
using MvApplication.UseCases.SearchMovies;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowtimesController(IMediator mediator) : ControllerBase {
  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken ct) {
    var query = new GetShowtimeByIdQuery(id);
    var result = await mediator.Send(query, ct);
    return Ok(result);
  }

  [HttpGet("search")]
  public async Task<IActionResult> Search([FromQuery] SearchMoviesQuery query, CancellationToken ct) {
    var result = await mediator.Send(query, ct);
    return Ok(result);
  }

  [HttpGet("{id:guid}/details")]
  public async Task<IActionResult> GetFullDetails(Guid id, CancellationToken ct) {
    var query = new GetShowtimeByIdQuery(id);
    var result = await mediator.Send(query, ct);
    return Ok(result);
  }
}

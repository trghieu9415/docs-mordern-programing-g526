using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvApplication.Ports;
using MvApplication.UseCases.GetProfile;
using MvApplication.UseCases.Login;
using MvApplication.UseCases.Register;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController(IMediator mediator) : ControllerBase {
  [HttpPost("login")]
  [AllowAnonymous]
  public async Task<IActionResult> Book([FromBody] LoginCommand command, CancellationToken ct) {
    var result = await mediator.Send(command, ct);
    return Ok(result);
  }

  [HttpPost("register")]
  [AllowAnonymous]
  public async Task<IActionResult> Book([FromBody] RegisterCommand command, CancellationToken ct) {
    var result = await mediator.Send(command, ct);
    return Ok(result);
  }

  [HttpGet("self")]
  public async Task<IActionResult> Book(ICurrentUser currentUser, CancellationToken ct) {
    var query = new GetProfileQuery(currentUser.Id);
    var result = await mediator.Send(query, ct);
    return Ok(result);
  }
}

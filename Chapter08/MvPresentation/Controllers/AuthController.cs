using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.Login;
using MvApplication.UseCases.Register;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController(IMediator mediator) : ControllerBase {
  [HttpPost("login")]
  public async Task<IActionResult> Book([FromBody] LoginCommand command, CancellationToken ct) {
    var result = await mediator.Send(command, ct);
    return Ok(result);
  }

  [HttpPost("register")]
  public async Task<IActionResult> Book([FromBody] RegisterCommand command, CancellationToken ct) {
    var result = await mediator.Send(command, ct);
    return Ok(result);
  }
}

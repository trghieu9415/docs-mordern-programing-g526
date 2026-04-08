using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.Ports;
using MvApplication.UseCases.CreateOrder;
using MvApplication.UseCases.GetOrders;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class OrderController(IMediator mediator) : ControllerBase {
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, ICurrentUser currentUser,
    CancellationToken ct) {
    command = command with { UserId = currentUser.Id, CustomerEmail = currentUser.Email };
    var result = await mediator.Send(command, ct);
    return Ok(result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll([FromBody] GetOrdersQuery query, CancellationToken ct) {
    var result = await mediator.Send(query, ct);
    return Ok(result);
  }
}

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

  [HttpPatch]
  public async Task<IActionResult> Test([FromServices] IBackgroundTaskQueue queue, CancellationToken ct) {
    var start = DateTime.Now;

    await queue.QueueAsync(async token => {
      await Task.Delay(2000, token);
      var endBackground = DateTime.Now;
      var duration = (endBackground - start).TotalMilliseconds;
      Console.WriteLine($"Task Completed In: {duration}ms");
    });

    var endApi = DateTime.Now;
    var durationApi = (endApi - start).TotalMilliseconds;
    return Ok($"API Completed In: {durationApi}ms");
  }
}

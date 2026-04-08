using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.Orders.CreateCheckout;
using MvApplication.UseCases.Orders.GetOrder;
using MvApplication.UseCases.Orders.HandleWebhook;
using MvApplication.UseCases.Orders.VerifyPaymentReturn;
using MvDomain.Entities;
using MvPresentation.Response;

namespace MvPresentation.Controllers;

[ApiController]
[Route("api/ticket-orders")]
[ApiExplorerSettings(GroupName = "v1")]
public class TicketOrdersController(IMediator mediator) : ControllerBase {
  [HttpPost("checkout")]
  public async Task<IActionResult> CreateCheckout([FromBody] CreateCheckoutCommand command, CancellationToken ct) {
    var result = await mediator.Send(command, ct);
    return AppResponse.Success(result, "Tao giao dich thanh toan thanh cong", 201);
  }

  [HttpGet("{orderId:guid}")]
  public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken ct) {
    var result = await mediator.Send(new GetOrderQuery(orderId), ct);
    return AppResponse.Success(result);
  }

  [HttpGet("return")]
  public async Task<IActionResult> HandleReturn([FromQuery] Guid orderId, CancellationToken ct) {
    var callbackData = Request.Query
      .ToDictionary(
        pair => pair.Key,
        pair => pair.Value.ToString(),
        StringComparer.OrdinalIgnoreCase);

    var result = await mediator.Send(new VerifyPaymentReturnCommand(orderId, callbackData), ct);
    var message = result.IsDuplicate
      ? result.Message
      : result.IsSuccess
        ? "Thanh toan thanh cong"
        : "Thanh toan that bai";

    return AppResponse.Success(result, message);
  }

  [HttpPost("webhook/{provider}")]
  public async Task<IActionResult> HandleWebhook(string provider, CancellationToken ct) {
    var resolvedProvider = ParseProvider(provider);
    Request.EnableBuffering();

    using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
    var payload = await reader.ReadToEndAsync(ct);
    Request.Body.Position = 0;

    var headers = Request.Headers.ToDictionary(
      pair => pair.Key,
      pair => pair.Value.ToString(),
      StringComparer.OrdinalIgnoreCase);

    var result = await mediator.Send(new HandleWebhookCommand(resolvedProvider, payload, headers), ct);
    return AppResponse.Success(result, result.Message);
  }

  [HttpGet("cancel")]
  public IActionResult Cancel([FromQuery] Guid orderId) {
    return AppResponse.Fail(new { OrderId = orderId }, "Nguoi dung da huy thanh toan.", 400);
  }

  private static PaymentProvider ParseProvider(string provider) {
    return provider.ToLowerInvariant() switch {
      "stripe" => PaymentProvider.Stripe,
      "paypal" => PaymentProvider.PayPal,
      _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Payment provider khong hop le.")
    };
  }
}

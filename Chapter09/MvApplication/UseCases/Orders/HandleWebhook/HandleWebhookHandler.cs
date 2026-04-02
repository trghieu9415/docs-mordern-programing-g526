using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.Orders.HandleWebhook;

public class HandleWebhookHandler(
  ITicketOrderRepository ticketOrderRepository,
  IEventRepository eventRepository,
  IPaymentServiceFactory paymentServiceFactory,
  IEmailService emailService
) : IRequestHandler<HandleWebhookCommand, HandleWebhookResult> {
  public async Task<HandleWebhookResult> Handle(HandleWebhookCommand request, CancellationToken cancellationToken) {
    var webhookResult = await paymentServiceFactory
      .Create(request.Provider)
      .ParseWebhookAsync(request.Payload, request.Headers, cancellationToken);

    if (webhookResult is null) {
      return new HandleWebhookResult(false, false, "Webhook khong can xu ly.", null);
    }

    var order = webhookResult.OrderId.HasValue
      ? await ticketOrderRepository.GetByIdAsync(webhookResult.OrderId.Value, cancellationToken)
      : null;

    if (order is null && !string.IsNullOrWhiteSpace(webhookResult.GatewayReferenceId)) {
      order = await ticketOrderRepository.GetByGatewayReferenceIdAsync(webhookResult.GatewayReferenceId, cancellationToken);
    }

    if (order is null) {
      throw new AppException("Khong tim thay don ve tu webhook.", 404);
    }

    if (order.IsFinalized()) {
      return new HandleWebhookResult(false, true, "Don hang da duoc xu ly truoc do.", order.Id);
    }

    var eventEntity = await eventRepository.GetByIdAsync(order.EventId, cancellationToken)
                     ?? throw new AppException("Khong tim thay su kien.", 404);

    if (webhookResult.IsSuccess) {
      var ticketCode = order.TicketCode ?? GenerateTicketCode(order.Id);
      order.MarkAsPaid(webhookResult.GatewayTransactionId ?? order.GatewayReferenceId ?? order.Id.ToString("N"), ticketCode);
      await ticketOrderRepository.UpdateAsync(order, cancellationToken);
      await emailService.SendTicketIssuedAsync(order.CustomerEmail, order.EventName, ticketCode, order.Id, order.Quantity, cancellationToken);
      return new HandleWebhookResult(true, true, "Webhook da xac nhan thanh toan thanh cong.", order.Id);
    }

    if (order.IsPending()) {
      eventEntity.ReleaseTickets(order.Quantity);
      await eventRepository.UpdateAsync(eventEntity, cancellationToken);
    }

    order.MarkAsFailed(webhookResult.Message);
    await ticketOrderRepository.UpdateAsync(order, cancellationToken);

    return new HandleWebhookResult(true, false, webhookResult.Message ?? "Webhook bao thanh toan that bai.", order.Id);
  }

  private static string GenerateTicketCode(Guid orderId) {
    return $"TICKET-{orderId.ToString("N")[..10].ToUpperInvariant()}";
  }
}

using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.Orders.VerifyPaymentReturn;

public class VerifyPaymentReturnHandler(
  ITicketOrderRepository ticketOrderRepository,
  IEventRepository eventRepository,
  IPaymentServiceFactory paymentServiceFactory,
  IEmailService emailService
) : IRequestHandler<VerifyPaymentReturnCommand, VerifyPaymentReturnResult> {
  public async Task<VerifyPaymentReturnResult> Handle(VerifyPaymentReturnCommand request, CancellationToken cancellationToken) {
    var order = await ticketOrderRepository.GetByIdAsync(request.OrderId, cancellationToken);
    if (order is null) {
      throw new AppException("Khong tim thay don ve.", 404);
    }

    if (order.IsFinalized()) {
      return new VerifyPaymentReturnResult(
        order.Id,
        true,
        order.Status.ToString(),
        order.TicketCode,
        order.GatewayTransactionId,
        "Don hang da duoc xac nhan truoc do.",
        true);
    }

    var verification = await paymentServiceFactory
      .Create(order.PaymentProvider)
      .VerifyReturnAsync(order, request.CallbackData, cancellationToken);

    var eventEntity = await eventRepository.GetByIdAsync(order.EventId, cancellationToken)
                     ?? throw new AppException("Khong tim thay su kien.", 404);

    if (verification.IsSuccess) {
      var ticketCode = order.TicketCode ?? GenerateTicketCode(order.Id);
      order.MarkAsPaid(verification.GatewayTransactionId ?? order.GatewayReferenceId ?? order.Id.ToString("N"), ticketCode);
      await ticketOrderRepository.UpdateAsync(order, cancellationToken);
      await emailService.SendTicketIssuedAsync(order.CustomerEmail, order.EventName, ticketCode, order.Id, order.Quantity, cancellationToken);
    } else {
      if (order.IsPending()) {
        eventEntity.ReleaseTickets(order.Quantity);
        await eventRepository.UpdateAsync(eventEntity, cancellationToken);
      }

      order.MarkAsFailed(verification.Message);
      await ticketOrderRepository.UpdateAsync(order, cancellationToken);
    }

    return new VerifyPaymentReturnResult(
      order.Id,
      verification.IsSuccess,
      order.Status.ToString(),
      order.TicketCode,
      order.GatewayTransactionId,
      verification.Message,
      false);
  }

  private static string GenerateTicketCode(Guid orderId) {
    return $"TICKET-{orderId.ToString("N")[..10].ToUpperInvariant()}";
  }
}

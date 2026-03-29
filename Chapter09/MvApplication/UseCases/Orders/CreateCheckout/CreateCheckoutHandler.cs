using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;
using MvDomain.Entities;

namespace MvApplication.UseCases.Orders.CreateCheckout;

public class CreateCheckoutHandler(
  IEventRepository eventRepository,
  ITicketOrderRepository ticketOrderRepository,
  IPaymentServiceFactory paymentServiceFactory
) : IRequestHandler<CreateCheckoutCommand, CreateCheckoutResult> {
  public async Task<CreateCheckoutResult> Handle(CreateCheckoutCommand request, CancellationToken cancellationToken) {
    var eventEntity = await eventRepository.GetByIdAsync(request.EventId, cancellationToken);
    if (eventEntity is null) {
      throw new AppException("Khong tim thay su kien.", 404);
    }

    eventEntity.ReserveTickets(request.Quantity);

    var order = TicketOrder.Create(
      eventEntity.Id,
      eventEntity.Name,
      request.CustomerEmail,
      request.Quantity,
      eventEntity.TicketPrice * request.Quantity,
      request.PaymentProvider);

    var paymentService = paymentServiceFactory.Create(request.PaymentProvider);
    var checkout = await paymentService.CreateCheckoutAsync(order, cancellationToken);

    order.SetCheckout(checkout.PaymentUrl, checkout.GatewayReferenceId);

    await ticketOrderRepository.AddAsync(order, cancellationToken);
    await eventRepository.UpdateAsync(eventEntity, cancellationToken);

    return new CreateCheckoutResult(
      order.Id,
      checkout.PaymentUrl,
      order.PaymentProvider.ToString(),
      order.Status.ToString());
  }
}

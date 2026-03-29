using MediatR;
using MvDomain.Entities;

namespace MvApplication.UseCases.Orders.CreateCheckout;

public record CreateCheckoutCommand(
  Guid EventId,
  string CustomerEmail,
  int Quantity,
  PaymentProvider PaymentProvider
) : IRequest<CreateCheckoutResult>;

using MediatR;
using MvDomain.Entities;

namespace MvApplication.UseCases.Orders.HandleWebhook;

public record HandleWebhookCommand(
  PaymentProvider Provider,
  string Payload,
  Dictionary<string, string> Headers
) : IRequest<HandleWebhookResult>;

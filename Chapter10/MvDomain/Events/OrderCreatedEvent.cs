using MvDomain.Base;

namespace MvDomain.Events;

public record OrderCreatedEvent(
  Guid OrderId,
  Guid UserId,
  decimal TotalAmount,
  string CustomerEmail
) : DomainEvent {
  public override Guid AggregateId => OrderId;
}

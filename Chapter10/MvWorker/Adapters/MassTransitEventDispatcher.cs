using MassTransit;
using MvApplication.Ports;
using MvDomain.Base;
using MvInfrastructure.Persistence;

namespace MvWorker.Adapters;

public class MassTransitEventDispatcher(
  AppDbContext dbContext,
  IPublishEndpoint publishEndpoint
) : IEventDispatcher {
  public async Task DispatchEventsAsync(CancellationToken ct = default) {
    var domainEntities = dbContext.ChangeTracker
      .Entries<BaseEntity>()
      .Where(x => x.Entity.DomainEvents.Count != 0)
      .Select(x => x.Entity)
      .ToList();

    if (domainEntities.Count == 0) {
      return;
    }
    
    var domainEvents = domainEntities
      .SelectMany(x => x.DomainEvents)
      .ToList();

    foreach (var domainEvent in domainEvents) {
      await publishEndpoint.Publish((object)domainEvent, ct);
    }

    domainEntities.ForEach(x => x.ClearEvents());
  }
}

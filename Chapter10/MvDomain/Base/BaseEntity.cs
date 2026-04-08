using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MvDomain.Base;

public class BaseEntity {
  private readonly List<DomainEvent> _domainEvents = [];
  public Guid Id { get; private init; } = Guid.NewGuid();
  public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
  [NotMapped] [JsonIgnore] public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  public void AddDomainEvent(DomainEvent domainEvent) {
    _domainEvents.Add(domainEvent);
  }

  public void ClearEvents() {
    _domainEvents.Clear();
  }
}

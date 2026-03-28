namespace MvDomain.Base;

public class BaseEntity {
  public Guid Id { get; private init; } = Guid.NewGuid();
  public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
}

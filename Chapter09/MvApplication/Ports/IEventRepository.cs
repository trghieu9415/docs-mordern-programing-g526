using MvDomain.Entities;

namespace MvApplication.Ports;

public interface IEventRepository {
  Task AddAsync(Event entity, CancellationToken ct = default);
  Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<IList<Event>> GetAllAsync(CancellationToken ct = default);
  Task UpdateAsync(Event entity, CancellationToken ct = default);
}

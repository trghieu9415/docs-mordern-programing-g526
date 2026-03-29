using MvDomain.Entities;

namespace MvApplication.Ports;

public interface ITicketOrderRepository {
  Task AddAsync(TicketOrder entity, CancellationToken ct = default);
  Task<TicketOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<TicketOrder?> GetByGatewayReferenceIdAsync(string gatewayReferenceId, CancellationToken ct = default);
  Task<IList<TicketOrder>> GetAllAsync(CancellationToken ct = default);
  Task UpdateAsync(TicketOrder entity, CancellationToken ct = default);
}

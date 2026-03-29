using Microsoft.EntityFrameworkCore;
using MvApplication.Ports;
using MvDomain.Entities;
using MvInfrastructure.Data;

namespace MvInfrastructure.Adapters;

public class TicketOrderRepository(TicketingDbContext dbContext) : ITicketOrderRepository {
  public async Task AddAsync(TicketOrder entity, CancellationToken ct = default) {
    dbContext.TicketOrders.Add(entity);
    await dbContext.SaveChangesAsync(ct);
  }

  public Task<TicketOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) {
    return dbContext.TicketOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
  }

  public Task<TicketOrder?> GetByGatewayReferenceIdAsync(string gatewayReferenceId, CancellationToken ct = default) {
    return dbContext.TicketOrders.FirstOrDefaultAsync(x => x.GatewayReferenceId == gatewayReferenceId, ct);
  }

  public async Task<IList<TicketOrder>> GetAllAsync(CancellationToken ct = default) {
    return await dbContext.TicketOrders
      .OrderByDescending(x => x.CreatedAt)
      .ToListAsync(ct);
  }

  public async Task UpdateAsync(TicketOrder entity, CancellationToken ct = default) {
    dbContext.TicketOrders.Update(entity);
    await dbContext.SaveChangesAsync(ct);
  }
}

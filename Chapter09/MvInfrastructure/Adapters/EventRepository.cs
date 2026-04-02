using Microsoft.EntityFrameworkCore;
using MvApplication.Ports;
using MvDomain.Entities;
using MvInfrastructure.Data;

namespace MvInfrastructure.Adapters;

public class EventRepository(TicketingDbContext dbContext) : IEventRepository {
  public async Task AddAsync(Event entity, CancellationToken ct = default) {
    dbContext.Events.Add(entity);
    await dbContext.SaveChangesAsync(ct);
  }

  public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default) {
    return dbContext.Events.FirstOrDefaultAsync(x => x.Id == id, ct);
  }

  public async Task<IList<Event>> GetAllAsync(CancellationToken ct = default) {
    return await dbContext.Events
      .OrderBy(x => x.StartAt)
      .ToListAsync(ct);
  }

  public async Task UpdateAsync(Event entity, CancellationToken ct = default) {
    dbContext.Events.Update(entity);
    await dbContext.SaveChangesAsync(ct);
  }
}

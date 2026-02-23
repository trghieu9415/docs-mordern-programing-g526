using Microsoft.EntityFrameworkCore.Storage;
using MvApplication.Ports;
using MvInfrastructure.Data;
using MvInfrastructure.Repositories;

namespace MvInfrastructure;

public class UnitOfWork(AppDbContext context) : IUnitOfWork {
  
  private IProductManager? _products;
  public IProductManager Products => _products ??= new ProductManager(context);
  
  public async Task<int> SaveChangesAsync(CancellationToken ct = default) {
    return await context.SaveChangesAsync(ct);
  }
  public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) {
    return await context.Database.BeginTransactionAsync(ct);
  }
}

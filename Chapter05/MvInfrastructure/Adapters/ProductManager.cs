using Microsoft.EntityFrameworkCore;
using MvApplication.Ports;
using MvDomain.Entities;
using MvInfrastructure.Data;

namespace MvInfrastructure.Repositories;

public class ProductManager(AppDbContext context) : IProductManager {
  
  public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) {
    return await context.Products
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Id == id, ct);
  }
  public async Task<Product?> GetByIdWithLockAsync(Guid id, CancellationToken ct = default) {
    return await context.Products
      .FromSql($"SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = {id}")
      .FirstOrDefaultAsync(ct);
  }
  
  public async Task<Product?> GetByIdWithTrackingAsync(Guid id, CancellationToken ct = default) {
    return await context.Products
      .FirstOrDefaultAsync(p => p.Id == id, ct);
  }
  
  public async Task<(IList<Product> Products, int Total)> GetPagedAsync(
    int page, int pageSize, CancellationToken ct = default) {

    var query = context.Products.AsNoTracking();

    var total = await query.CountAsync(ct);

    var items = await query
      .OrderBy(p => p.Name)      
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(ct);

    return (items, total);
  }

  public async Task AddAsync(Product product, CancellationToken ct = default) {
    await context.Products.AddAsync(product, ct);
  }
  
  public void Update(Product product) {
    context.Products.Update(product);
  }

  public void Delete(Product product) {
    context.Products.Remove(product);
  }


  public async Task LoadRelationsExplicitlyAsync(Product product, CancellationToken ct = default) {
    await context.Entry(product).Collection(p => p.Tags).LoadAsync(ct);
    await context.Entry(product).Reference(p => p.Category).LoadAsync(ct);
    await context.Entry(product).Reference(p => p.Detail).LoadAsync(ct);
  }
  
  public async Task<Product?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default) {
    return await context.Products
      .AsNoTracking()
      .Include(p => p.Category)
      .Include(p => p.Tags)
      .Include(p => p.Detail)
      .FirstOrDefaultAsync(p => p.Id == id, ct);
  }
  
  public async Task<(IList<Product> Products, int Total)> GetPagedWithCategoryAsync(
    int page, int pageSize, CancellationToken ct = default) {
    var query = context.Products
      .AsNoTracking()
      .Include(p => p.Category); 

    var total = await query.CountAsync(ct);
    var items = await query
      .OrderBy(p => p.Name)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(ct);

    return (items, total);
  }
  
  public async Task<(IList<Product> Products, int Total)> GetPagedByCategoryAsync(
    int categoryId, int page, int pageSize, CancellationToken ct = default) {
    var query = context.Products
      .AsNoTracking()
      .Include(p => p.Category)
      .Where(p => p.CategoryId == categoryId); 

    var total = await query.CountAsync(ct);
    var items = await query
      .OrderBy(p => p.Name)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(ct);

    return (items, total);
  }
  
}

using MvApplication.Ports;
using Microsoft.EntityFrameworkCore;
using MvDomain.Entities;
using MvInfrastructure.Persistence;

namespace MvInfrastructure.Adapters;

public class ProductManager(AppDbContext db) : IProductManager {
  public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) {
    return await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
  }

  public async Task<(IList<Product> Products, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct) {
    var total = await db.Products.CountAsync(ct);
    var items = await db.Products
      .OrderBy(p => p.Name)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(ct);

    return (items, total);
  }


  public async Task AddAsync(Product product, CancellationToken ct) {
    db.Products.Add(product);
    await db.SaveChangesAsync(ct);
  }

  public async Task UpdateAsync(Product product, CancellationToken ct) {
    db.Products.Update(product);
    await db.SaveChangesAsync(ct);
  }

  public async Task DeleteAsync(Guid id, CancellationToken ct) {
    var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
    if (existing != null) {
      db.Products.Remove(existing);
      await db.SaveChangesAsync(ct);
    }
  }

  public async Task<Order> CreateOrderAsync(Guid productId, string userId, int quantity, CancellationToken ct) {
    var order = Order.Create(productId, userId, quantity);
    db.Orders.Add(order);
    await db.SaveChangesAsync(ct);
    return order;
  }

  public async Task<IReadOnlyList<Order>> GetOrdersByProductIdAsync(Guid productId, CancellationToken ct) {
    return await db.Orders
      .AsNoTracking()
      .Where(o => o.ProductId == productId)
      .OrderBy(o => o.CreatedAt)
      .ToListAsync(ct);
  }
}

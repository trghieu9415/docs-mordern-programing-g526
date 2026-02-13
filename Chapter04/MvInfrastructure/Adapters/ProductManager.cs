using MvApplication.Ports;
using MvDomain.Entities;
using MvInfrastructure.Store;

namespace MvInfrastructure.Adapters;

public class ProductManager(ProductStore store) : IProductManager {
  public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) {
    return await Task.FromResult(store.Products.FirstOrDefault(p => p.Id == id));
  }

  public async Task<(IList<Product> Products, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct) {
    var total = store.Products.Count;
    var items = store.Products
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToList();

    return await Task.FromResult((items.ToList(), total));
  }


  public async Task AddAsync(Product product, CancellationToken ct) {
    store.Products.Add(product);
    await Task.CompletedTask;
  }

  public async Task UpdateAsync(Product product, CancellationToken ct) {
    var existing = store.Products.FirstOrDefault(p => p.Id == product.Id);
    if (existing != null) {
      store.Products.Remove(existing);
      store.Products.Add(product);
    }

    await Task.CompletedTask;
  }

  public async Task DeleteAsync(Guid id, CancellationToken ct) {
    var existing = store.Products.FirstOrDefault(p => p.Id == id);
    if (existing != null) {
      store.Products.Remove(existing);
    }

    await Task.CompletedTask;
  }
}

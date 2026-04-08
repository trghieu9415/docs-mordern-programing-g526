using Microsoft.EntityFrameworkCore;
using MvApplication.Repositories;
using MvDomain.Entities;
using MvInfrastructure.Data;

namespace MvInfrastructure.Repositories;

public class CategoryRepository(LibraryDbContext context) : ICategoryRepository {
  public async Task<IReadOnlyCollection<Category>> GetByIdsAsync(
    IReadOnlyCollection<int> categoryIds,
    CancellationToken ct = default
  ) {
    if (categoryIds.Count == 0) {
      return [];
    }

    return await context.Categories
      .Where(category => categoryIds.Contains(category.Id))
      .OrderBy(category => category.Id)
      .ToListAsync(ct);
  }
}

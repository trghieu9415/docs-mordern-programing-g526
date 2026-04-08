using Microsoft.EntityFrameworkCore;
using MvDomain.Entities;

namespace MvInfrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
  public DbSet<Product> Products => Set<Product>();
  public DbSet<Order> Orders => Set<Order>();

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Product>(entity => {
      entity.HasKey(x => x.Id);
      entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
      entity.Property(x => x.Price).HasPrecision(18, 2);
      entity.Property(x => x.AvailableStock).IsRequired();
      entity.Property(x => x.ImageUrl).HasMaxLength(500);

      entity.HasMany(x => x.Orders)
        .WithOne(x => x.Product)
        .HasForeignKey(x => x.ProductId);
    });

    modelBuilder.Entity<Order>(entity => {
      entity.HasKey(x => x.Id);
      entity.Property(x => x.UserId).HasMaxLength(100).IsRequired();
      entity.Property(x => x.Quantity).IsRequired();
      entity.Property(x => x.CreatedAt).IsRequired();
    });
  }
}

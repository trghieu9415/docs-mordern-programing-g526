using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category> {
  public void Configure(EntityTypeBuilder<Category> builder) {
    builder.ToTable("Categories");
    builder.HasKey(c => c.Id);
    builder.Property(c => c.Id).ValueGeneratedOnAdd(); // int Id — SQL tự tăng

    builder.Property(c => c.Name)
      .IsRequired()
      .HasMaxLength(100);

    // Quan hệ 1-n: Category (1) ──< Product (nhiều)
    // Fluent API: HasMany + WithOne
    builder.HasMany(c => c.Products)
      .WithOne(p => p.Category)
      .HasForeignKey(p => p.CategoryId)
      .OnDelete(DeleteBehavior.SetNull); // Xóa category → CategoryId của product = NULL
  }
}

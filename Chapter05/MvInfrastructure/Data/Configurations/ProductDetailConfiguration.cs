using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Data.Configurations;

public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail> {
  public void Configure(EntityTypeBuilder<ProductDetail> builder) {
    builder.ToTable("ProductDetails");
    builder.HasKey(d => d.Id);
    builder.Property(d => d.Id).ValueGeneratedNever();

    builder.Property(d => d.Description).IsRequired();
    builder.Property(d => d.Specification).HasMaxLength(500);

    // Quan hệ 1-1: Product (1) ── ProductDetail (1)
    // Fluent API: HasOne + WithOne + HasForeignKey
    builder.HasOne(d => d.Product)
      .WithOne(p => p.Detail)
      .HasForeignKey<ProductDetail>(d => d.ProductId) // ProductDetail giữ FK
      .OnDelete(DeleteBehavior.Cascade); // Xóa Product → xóa luôn ProductDetail
  }
}

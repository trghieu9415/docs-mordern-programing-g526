using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag> {
  public void Configure(EntityTypeBuilder<Tag> builder) {
    builder.ToTable("Tags");
    builder.HasKey(t => t.Id);
    builder.Property(t => t.Id).ValueGeneratedOnAdd();
    builder.Property(t => t.Name).IsRequired().HasMaxLength(50);

    // Quan hệ n-n: Product ──< ProductTag >── Tag
    // EF Core 5+ tự tạo bảng join "ProductTag" với 2 FK
    builder.HasMany(t => t.Products)
      .WithMany(p => p.Tags)
      .UsingEntity(join => join.ToTable("ProductTags")); // Đặt tên bảng join rõ ràng
  }
}

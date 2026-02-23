using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product> {
  public void Configure(EntityTypeBuilder<Product> builder) {
    builder.ToTable("Products");

    builder.HasKey(p => p.Id);

    // Guid được tạo bởi domain (Guid.NewGuid()), không để SQL tự sinh
    builder.Property(p => p.Id)
      .ValueGeneratedNever();

    builder.Property(p => p.Name)
      .IsRequired()
      .HasMaxLength(200);

    // Dùng decimal(18,2) để lưu tiền tệ chính xác, tránh lỗi làm tròn float
    builder.Property(p => p.Price)
      .HasColumnType("decimal(18,2)")
      .IsRequired();

    builder.Property(p => p.Stock)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(p => p.ImageUrl)
      .HasMaxLength(500);

    // Cấu hình Optimistic Concurrency Token
    // IsRowVersion() đã tự động bao gồm IsConcurrencyToken() trong EF Core
    // Gọi tường minh .IsConcurrencyToken() để code rõ ý định hơn
    builder.Property(p => p.RowVersion)
      .IsRowVersion();        // Tạo cột ROWVERSION trong SQL Server + tự động làm ConcurrencyToken

    // Index để tăng tốc tìm kiếm/sắp xếp theo tên
    builder.HasIndex(p => p.Name);
  }
}

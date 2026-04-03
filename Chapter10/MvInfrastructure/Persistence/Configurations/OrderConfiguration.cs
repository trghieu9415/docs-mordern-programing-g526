using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Persistence.Configurations;

public class OrderConfiguration : BaseConfiguration<Order> {
  public override void Configure(EntityTypeBuilder<Order> builder) {
    builder.ToTable("Orders");
    base.Configure(builder);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.CustomerEmail)
      .IsRequired()
      .HasMaxLength(255);

    builder.Property(x => x.TotalAmount)
      .IsRequired()
      .HasPrecision(18, 2);

    builder.Property(x => x.Status)
      .IsRequired()
      .HasConversion<string>();

    builder.HasIndex(x => x.CustomerEmail);
  }
}

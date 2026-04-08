using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Persistence.Configurations;

public class UserConfiguration : BaseConfiguration<User> {
  public override void Configure(EntityTypeBuilder<User> builder) {
    builder.ToTable("Users");
    base.Configure(builder);

    builder.Property(x => x.Username)
      .IsRequired()
      .HasMaxLength(100);

    builder.HasIndex(x => x.Username)
      .IsUnique();

    builder.Property(x => x.Email)
      .IsRequired()
      .HasMaxLength(255);

    builder.HasIndex(x => x.Email)
      .IsUnique();

    builder.Property(x => x.PlainPassword)
      .IsRequired()
      .HasMaxLength(500);

    builder.Property(x => x.CumulativePoint)
      .IsRequired()
      .HasDefaultValue(0);
  }
}

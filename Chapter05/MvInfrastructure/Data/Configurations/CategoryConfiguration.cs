using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category> {
  public void Configure(EntityTypeBuilder<Category> builder) {
    builder.ToTable("Category");

    builder.HasKey(category => category.Id);

    builder.Property(category => category.Id)
      .UseIdentityByDefaultColumn();

    builder.Property(category => category.Name)
      .HasMaxLength(150)
      .IsRequired();
  }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Data.Configurations;

public class BookDetailConfiguration : IEntityTypeConfiguration<BookDetail> {
  public void Configure(EntityTypeBuilder<BookDetail> builder) {
    builder.ToTable("BookDetail");

    builder.HasKey(detail => detail.Id);

    builder.Property(detail => detail.Id)
      .UseIdentityByDefaultColumn();

    builder.Property(detail => detail.Summary)
      .HasMaxLength(2000)
      .IsRequired();

    builder.Property(detail => detail.IsEbook)
      .IsRequired();

    builder.HasIndex(detail => detail.BookId)
      .IsUnique();
  }
}

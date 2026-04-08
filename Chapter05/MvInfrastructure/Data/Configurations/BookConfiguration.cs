using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Entities;

namespace MvInfrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book> {
  public void Configure(EntityTypeBuilder<Book> builder) {
    builder.ToTable("Book");

    builder.HasKey(book => book.Id);

    builder.Property(book => book.Id)
      .UseIdentityByDefaultColumn();

    builder.Property(book => book.Title)
      .HasMaxLength(255)
      .IsRequired();

    builder.Property(book => book.RowVersion)
      .IsRowVersion();

    builder.HasOne(book => book.BookDetail)
      .WithOne(detail => detail.Book)
      .HasForeignKey<BookDetail>(detail => detail.BookId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(book => book.Categories)
      .WithMany(category => category.Books)
      .UsingEntity<Dictionary<string, object>>(
        "BookCategory",
        right => right.HasOne<Category>()
          .WithMany()
          .HasForeignKey("CategoryId")
          .OnDelete(DeleteBehavior.Cascade),
        left => left.HasOne<Book>()
          .WithMany()
          .HasForeignKey("BookId")
          .OnDelete(DeleteBehavior.Cascade),
        join => {
          join.ToTable("BookCategory");
          join.HasKey("BookId", "CategoryId");
        }
      );
  }
}

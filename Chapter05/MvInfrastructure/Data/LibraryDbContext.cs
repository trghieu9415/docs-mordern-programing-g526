using Microsoft.EntityFrameworkCore;
using MvDomain.Entities;

namespace MvInfrastructure.Data;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options) {
  public DbSet<Book> Books => Set<Book>();
  public DbSet<Category> Categories => Set<Category>();
  public DbSet<BookDetail> BookDetails => Set<BookDetail>();

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
    base.OnModelCreating(modelBuilder);
  }
}

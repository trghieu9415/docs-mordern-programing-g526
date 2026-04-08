using Microsoft.EntityFrameworkCore;
using MvApplication.Repositories;
using MvDomain.Entities;
using MvInfrastructure.Data;

namespace MvInfrastructure.Repositories;

public class BookReadRepository(LibraryDbContext context) : IBookReadRepository {
  public IQueryable<Book> GetAll() {
    return context.Books.AsNoTracking();
  }

  public IQueryable<Book> GetById(int id) {
    return context.Books
      .AsNoTracking()
      .Include(book => book.BookDetail)
      .Include(book => book.Categories)
      .Where(book => book.Id == id);
  }
}

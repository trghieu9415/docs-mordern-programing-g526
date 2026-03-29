using Microsoft.EntityFrameworkCore;
using MvApplication.Repositories;
using MvDomain.Entities;
using MvInfrastructure.Data;

namespace MvInfrastructure.Repositories;

public class BookRepository(LibraryDbContext context) : IBookRepository {
  public IQueryable<Book> GetAll() {
    return context.Books.AsNoTracking();
  }

  public IQueryable<Book> GetById(int id) {
    return context.Books
      .AsNoTracking()
      .Where(book => book.Id == id);
  }

  public IQueryable<Book> GetAllTracking() {
    return context.Books;
  }

  public IQueryable<Book> GetByIdTracking(int id) {
    return context.Books.Where(book => book.Id == id);
  }

  public void Add(Book book) {
    context.Books.Add(book);
  }

  public void Update(Book book) {
    if (context.Entry(book).State == EntityState.Detached) {
      context.Books.Attach(book);
    }
  }

  public void Delete(Book book) {
    if (context.Entry(book).State == EntityState.Detached) {
      context.Books.Attach(book);
    }

    context.Books.Remove(book);
  }

  public void SetOriginalRowVersion(Book book, uint rowVersion) {
    context.Entry(book).Property(item => item.RowVersion).OriginalValue = rowVersion;
  }
}

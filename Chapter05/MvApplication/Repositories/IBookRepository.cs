using MvDomain.Entities;

namespace MvApplication.Repositories;

public interface IBookRepository : IBookReadRepository {
  IQueryable<Book> GetAllTracking();
  IQueryable<Book> GetByIdTracking(int id);
  void Add(Book book);
  void Update(Book book);
  void Delete(Book book);
  void SetOriginalRowVersion(Book book, uint rowVersion);
}

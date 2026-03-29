using MvDomain.Entities;

namespace MvApplication.Repositories;

public interface IBookReadRepository {
  IQueryable<Book> GetAll();
  IQueryable<Book> GetById(int id);
}

using MvDomain.Exceptions;

namespace MvDomain.Entities;

public class Category {
  private Category() {}

  public int Id { get; private set; }
  public string Name { get; private set; } = null!;
  public ICollection<Book> Books { get; private set; } = [];

  public static Category Create(string name) {
    if (string.IsNullOrWhiteSpace(name)) {
      throw new DomainException("Tên thể loại không được để trống.");
    }

    return new Category {
      Name = name.Trim()
    };
  }
}

using MvDomain.Exceptions;

namespace MvDomain.Entities;

public class Book {
  private Book() {}

  public int Id { get; private set; }
  public string Title { get; private set; } = null!;
  public uint RowVersion { get; private set; }
  public BookDetail? BookDetail { get; private set; }
  public ICollection<Category> Categories { get; private set; } = [];

  public static Book Create(string title, string summary, bool isEbook, IEnumerable<Category> categories) {
    var book = new Book();
    book.Update(title, summary, isEbook);
    book.ReplaceCategories(categories);
    return book;
  }

  public void Update(string title, string summary, bool isEbook) {
    if (string.IsNullOrWhiteSpace(title)) {
      throw new DomainException("Tiêu đề sách không được để trống.");
    }

    if (string.IsNullOrWhiteSpace(summary)) {
      throw new DomainException("Tóm tắt sách không được để trống.");
    }

    Title = title.Trim();

    if (BookDetail is null) {
      BookDetail = BookDetail.Create(summary, isEbook);
      return;
    }

    BookDetail.Update(summary, isEbook);
  }

  public void ReplaceCategories(IEnumerable<Category> categories) {
    var distinctCategories = categories
      .DistinctBy(category => category.Id)
      .ToList();

    if (distinctCategories.Count == 0) {
      throw new DomainException("Sách phải thuộc ít nhất một thể loại.");
    }

    Categories.Clear();

    foreach (var category in distinctCategories) {
      Categories.Add(category);
    }
  }
}

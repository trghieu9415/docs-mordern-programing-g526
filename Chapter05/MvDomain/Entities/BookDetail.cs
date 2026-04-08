using MvDomain.Exceptions;

namespace MvDomain.Entities;

public class BookDetail {
  private BookDetail() {}

  public int Id { get; private set; }
  public string Summary { get; private set; } = null!;
  public bool IsEbook { get; private set; }
  public int BookId { get; private set; }
  public Book Book { get; private set; } = null!;

  public static BookDetail Create(string summary, bool isEbook) {
    var detail = new BookDetail();
    detail.Update(summary, isEbook);
    return detail;
  }

  public void Update(string summary, bool isEbook) {
    if (string.IsNullOrWhiteSpace(summary)) {
      throw new DomainException("Tóm tắt sách không được để trống.");
    }

    Summary = summary.Trim();
    IsEbook = isEbook;
  }
}

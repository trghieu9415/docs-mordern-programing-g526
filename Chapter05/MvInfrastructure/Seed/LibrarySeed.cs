using Microsoft.EntityFrameworkCore;
using MvApplication.Ports;
using MvDomain.Entities;
using MvInfrastructure.Data;

namespace MvInfrastructure.Seed;

public class LibrarySeed(LibraryDbContext context, IAppLogger<LibrarySeed> logger) {
  public async Task SeedAsync(CancellationToken ct = default) {
    if (await context.Books.AnyAsync(ct)) {
      return;
    }

    var softwareDesign = Category.Create("Software Design");
    var database = Category.Create("Database");
    var architecture = Category.Create("Architecture");

    var books = new[] {
      Book.Create(
        "Clean Architecture",
        "Nguyen tac va huong dan xay dung he thong de bao tri.",
        true,
        [softwareDesign, architecture]
      ),
      Book.Create(
        "Designing Data-Intensive Applications",
        "Tong hop cac mo hinh du lieu, replication va transaction trong he thong lon.",
        true,
        [database, architecture]
      ),
      Book.Create(
        "PostgreSQL Up and Running",
        "Sach nhap mon va toi uu van hanh PostgreSQL trong thuc te.",
        false,
        [database]
      ),
      Book.Create(
        "Patterns of Enterprise Application Architecture",
        "Mau thiet ke pho bien cho ung dung doanh nghiep.",
        true,
        [softwareDesign, architecture]
      )
    };

    await context.Categories.AddRangeAsync([softwareDesign, database, architecture], ct);
    await context.Books.AddRangeAsync(books, ct);
    await context.SaveChangesAsync(ct);

    logger.LogBusinessInformation(
      "Da seed thanh cong {BookCount} sach va {CategoryCount} the loai cho E-Library.",
      books.Length,
      3
    );
  }
}

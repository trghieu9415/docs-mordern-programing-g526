using MvDomain.Entities;

namespace MvInfrastructure.Persistence;

public static class AppDbInitializer {
  public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default) {
    if (db.Products.Any()) {
      return;
    }

    var products = new List<Product> {
      Product.Create("Laptop Gaming ASUS ROG", 35000000, "https://placehold.co/600x400").UpdateStock(12),
      Product.Create("Bàn phím cơ Akko v3", 1500000, "https://placehold.co/600x400").UpdateStock(12),
      Product.Create("Chuột Logitech G502", 1200000, "https://placehold.co/600x400").UpdateStock(12),
      Product.Create("Màn hình Dell UltraSharp", 8000000, null).UpdateStock(12),
      Product.Create("Tai nghe Sony WH-1000XM5", 9000000, "https://placehold.co/600x400").UpdateStock(12),
      // Dùng sản phẩm này cho demo tranh chấp: chỉ còn 1 tồn kho.
      Product.Create("Flash Sale Demo (stock=1)", 99000, null).UpdateStock(1)
    };

    db.Products.AddRange(products);
    await db.SaveChangesAsync(ct);
  }
}

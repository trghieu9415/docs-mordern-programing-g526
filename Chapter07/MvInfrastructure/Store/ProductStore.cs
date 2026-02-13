using MvDomain.Entities;

namespace MvInfrastructure.Store;

public class ProductStore {
  public List<Product> Products { get; } = [
    Product.Create("Laptop Gaming ASUS ROG", 35000000, "https://placehold.co/600x400").UpdateStock(12),
    Product.Create("Bàn phím cơ Akko v3", 1500000, "https://placehold.co/600x400").UpdateStock(12),
    Product.Create("Chuột Logitech G502", 1200000, "https://placehold.co/600x400").UpdateStock(12),
    Product.Create("Màn hình Dell UltraSharp", 8000000, null).UpdateStock(12),
    Product.Create("Tai nghe Sony WH-1000XM5", 9000000, "https://placehold.co/600x400").UpdateStock(12)
  ];
}

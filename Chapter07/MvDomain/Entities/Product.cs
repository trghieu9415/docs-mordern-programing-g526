namespace MvDomain.Entities;

public class Product {
  private Product() {}
  public Guid Id { get; private set; }
  public string Name { get; private set; } = null!;
  public decimal Price { get; private set; }
  public int AvailableStock { get; private set; }
  public string? ImageUrl { get; private set; }
  public ICollection<Order> Orders { get; private set; } = [];

  public static Product Create(string name, decimal price, string? imageUrl) {
    return new Product {
      Id = Guid.NewGuid(),
      Name = name,
      Price = price,
      AvailableStock = 0,
      ImageUrl = imageUrl
    };
  }

  public Product Update(string name, decimal price, string? imageUrl) {
    Name = name;
    Price = price;
    ImageUrl = imageUrl;
    return this;
  }

  public Product UpdateStock(int quantity) {
    if (AvailableStock + quantity < 0) {
      throw new InvalidOperationException("Số lượng tồn kho không thể âm.");
    }

    AvailableStock += quantity;
    return this;
  }
}

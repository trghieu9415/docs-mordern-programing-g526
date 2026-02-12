namespace MvDomain.Entities;

public class Product {
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public decimal Price { get; set; }
  public int Stock { get; set; }
  public string? ImageUrl { get; set; }
  
  public static Product Create(string name, decimal price, string? imageUrl) => new Product {
    Name = name,
    Price = price,
    Stock = 0,
    ImageUrl = imageUrl
  };

  public Product Update(string name, decimal price, string? imageUrl) {
    Name = name;
    Price = price;
    ImageUrl = imageUrl;
    return this;
  }

  public Product UpdateStock(int stock) {
    Stock = stock;
    return this;
  }
}

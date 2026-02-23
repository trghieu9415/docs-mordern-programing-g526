﻿
namespace MvDomain.Entities;

public class Product {
  protected Product() {}

  public Guid Id { get; private set; }
  public string Name { get; private set; } = null!;
  public decimal Price { get; private set; }
  public int Stock { get; private set; }
  public string? ImageUrl { get; private set; }
  public byte[] RowVersion { get; private set; } = null!;
  
  public int? CategoryId { get; private set; }
  public Category? Category { get; private set; }
  
  public ProductDetail? Detail { get; private set; }
  
  public ICollection<Tag> Tags { get; private set; } = new List<Tag>();


  public static Product Create(string name, decimal price, string? imageUrl, int? categoryId = null) {
    return new Product {
      Id = Guid.NewGuid(),
      Name = name,
      Price = price,
      Stock = 0,
      ImageUrl = imageUrl,
      CategoryId = categoryId
    };
  }

  public Product Update(string name, decimal price, string? imageUrl, int? categoryId) {
    Name = name;
    Price = price;
    ImageUrl = imageUrl;
    CategoryId = categoryId;
    return this;
  }

  public Product UpdateStock(int quantity) {
    if (Stock + quantity < 0)
      throw new InvalidOperationException("Số lượng tồn kho không thể âm.");
    Stock += quantity;
    return this;
  }
}

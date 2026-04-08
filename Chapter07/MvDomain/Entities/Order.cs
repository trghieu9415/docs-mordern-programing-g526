namespace MvDomain.Entities;

public class Order {
  private Order() {}

  public Guid Id { get; private set; }
  public Guid ProductId { get; private set; }
  public string UserId { get; private set; } = null!;
  public int Quantity { get; private set; }
  public DateTime CreatedAt { get; private set; }

  public Product Product { get; private set; } = null!;

  public static Order Create(Guid productId, string userId, int quantity) {
    return new Order {
      Id = Guid.NewGuid(),
      ProductId = productId,
      UserId = userId,
      Quantity = quantity,
      CreatedAt = DateTime.UtcNow
    };
  }
}

using System.ComponentModel.DataAnnotations;

namespace MvDomain.Entities;

public class ProductDetail {
  protected ProductDetail() {}

  public Guid Id { get; private set; }

  [Required]
  public string Description { get; private set; } = null!;

  [MaxLength(500)]
  public string? Specification { get; private set; }
  
  public Guid ProductId { get; private set; }
  public Product? Product { get; private set; }

  public static ProductDetail Create(Guid productId, string description, string? spec = null)
    => new() { Id = Guid.NewGuid(), ProductId = productId, Description = description, Specification = spec };
}

using System.ComponentModel.DataAnnotations;  // Data Annotations

namespace MvDomain.Entities;

public class Category {
  protected Category() {}

  public int Id { get; private set; }
  
  [Required]
  [MaxLength(100)]
  public string Name { get; private set; } = null!;
  
  public ICollection<Product> Products { get; private set; } = new List<Product>();

  public static Category Create(string name) => new() { Name = name };
}

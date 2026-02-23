using System.ComponentModel.DataAnnotations;

namespace MvDomain.Entities;

public class Tag {
  protected Tag() {}

  public int Id { get; private set; }

  [Required]
  [MaxLength(50)]
  public string Name { get; private set; } = null!;
  
  public ICollection<Product> Products { get; private set; } = new List<Product>();

  public static Tag Create(string name) => new() { Name = name };
}

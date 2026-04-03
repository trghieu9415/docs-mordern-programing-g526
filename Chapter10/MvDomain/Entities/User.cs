using MvDomain.Base;

namespace MvDomain.Entities;

public class User : BaseEntity {
  private User() {}
  public string Username { get; private set; } = null!;
  public string Email { get; private set; } = null!;
  public string PlainPassword { get; private set; } = null!;

  public int CumulativePoint { get; private set; }

  public static User Create(string username, string email, string plainPassword) {
    return new User {
      Username = username,
      Email = email,
      PlainPassword = plainPassword
    };
  }

  public void AddPoint(int point) {
    CumulativePoint += point;
  }
}

using MvDomain.Base;

namespace MvDomain.Entities;

public class User : BaseEntity {
  private User() {}
  public string Username { get; private set; } = null!;
  public string PlainPassword { get; private set; } = null!;

  public static User Create(string username, string plainPassword) {
    return new User {
      Username = username,
      PlainPassword = plainPassword
    };
  }
}

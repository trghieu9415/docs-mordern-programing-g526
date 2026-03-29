namespace MvApplication.Configs.Options;

public class JwtSettings : IOptionSection {
  public string Key { get; init; } = null!;
  public string Issuer { get; init; } = null!;
  public string Audience { get; init; } = null!;
  public int ExpiryInMinutes { get; init; }
  public static string SectionName => "Jwt";
}

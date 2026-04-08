namespace MvApplication.DTOs;

public record UserDto {
  public string Username { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public int CumulativePoint { get; init; }
}

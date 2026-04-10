namespace MvApplication.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string UserName, string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
  string AccessToken,
  DateTime AccessTokenExpiry,
  string RefreshToken,
  DateTime RefreshTokenExpiry
);

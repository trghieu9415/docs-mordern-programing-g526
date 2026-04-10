namespace MvApplication.Models;

public record TokenModel
{
    public string Token { get; init; } = null!;
    public DateTime ExpiredAt { get; init; }
}

public record AuthTokens(TokenModel Access, TokenModel Refresh);

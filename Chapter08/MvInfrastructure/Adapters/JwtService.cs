using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MvApplication.Configs.Options;
using MvApplication.Ports;
using MvDomain.Entities;

namespace MvInfrastructure.Adapters;

public class JwtService(JwtSettings settings) : IJwtService {
  public string GenerateToken(User user) {
    var claims = new List<Claim> {
      new(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new(ClaimTypes.Name, user.Username),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      settings.Issuer,
      settings.Audience,
      claims,
      expires: DateTime.Now.AddMinutes(settings.ExpiryInMinutes),
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public static TokenValidationParameters GetTokenValidationParameters(JwtSettings settings) {
    return new TokenValidationParameters {
      ValidateIssuer = true,
      ValidIssuer = settings.Issuer,
      ValidateAudience = true,
      ValidAudience = settings.Audience,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key))
    };
  }
}

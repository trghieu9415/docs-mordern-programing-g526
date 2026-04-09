using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MvApplication.Models;
using MvApplication.Ports.Security;
using MvInfrastructure.Options;
using Microsoft.IdentityModel.Tokens;

namespace MvPresentation.Adapters.Security;

public class JwtService(JwtOptions jwtOptions) : IJwtService
{
  public TokenModel GenerateAccessToken(User user)
  {
    var securityStamp = user.SecurityStamp
      ?? throw new InvalidOperationException("SecurityStamp không được rỗng khi tạo access token");

    var claims = new List<Claim> {
      new(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new(ClaimTypes.Name, user.UserName),
      new(ClaimTypes.Email, user.Email),
      new(ClaimTypes.Role, user.Role.ToString()),
      new(AuthClaimTypes.SecurityStamp, securityStamp),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expiry = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenExpiryMinutes);

    var token = new JwtSecurityToken(
      issuer: jwtOptions.Issuer,
      audience: jwtOptions.Audience,
      claims: claims,
      expires: expiry,
      signingCredentials: credentials
    );

    return new TokenModel
    {
      Token = new JwtSecurityTokenHandler().WriteToken(token),
      ExpiredAt = expiry
    };
  }

  public TokenModel GenerateRefreshToken(User user)
  {
    var securityStamp = user.SecurityStamp
      ?? throw new InvalidOperationException("SecurityStamp không được rỗng khi tạo refresh token");

    var claims = new List<Claim> {
      new(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new(AuthClaimTypes.SecurityStamp, securityStamp),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expiry = DateTime.UtcNow.AddMinutes(jwtOptions.RefreshTokenExpiryMinutes);

    var token = new JwtSecurityToken(
      issuer: jwtOptions.Issuer,
      audience: jwtOptions.Audience,
      claims: claims,
      expires: expiry,
      signingCredentials: credentials
    );

    return new TokenModel
    {
      Token = new JwtSecurityTokenHandler().WriteToken(token),
      ExpiredAt = expiry
    };
  }
}

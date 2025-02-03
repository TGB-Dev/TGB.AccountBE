using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(ApplicationUser user)
    {
        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Token:AccessToken:SigningKey"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GetClaimsIdentity(user),
            Expires = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Token:AccessToken:ExpiresInMinutes"]!)),
            Issuer = _configuration["Token:AccessToken:Issuer"],
            Audience = _configuration["Token:AccessToken:Audience"],
            SigningCredentials = credentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static ClaimsIdentity GetClaimsIdentity(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new(JwtRegisteredClaimNames.Name, user.DisplayName!),
            new(JwtRegisteredClaimNames.Email, user.Email!)
        };
        return new ClaimsIdentity(claims);
    }
}

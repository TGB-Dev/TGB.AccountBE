using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TGB.AccountBE.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    }

    public static string GetUserName(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.UniqueName)!;
    }

    public static string GetDisplayName(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Name)!;
    }

    public static string GetEmail(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Email)!;
    }
}

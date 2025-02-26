using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IOidcAuthService
{
    Task<ClaimsPrincipal> Authorize(OpenIddictRequest request, string userId);
    Task<IActionResult> Logout();
    Task<ClaimsPrincipal> Exchange(OpenIddictRequest request, string userId, ClaimsPrincipal principal);
    Task<IActionResult> UserInfo(string userId);

    // These are for accepting and denying applications to be authorized
    Task<IActionResult> Accept();
    Task<IActionResult> Deny();
}

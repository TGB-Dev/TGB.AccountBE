using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IOidcAuthService
{
    Task<ClaimsPrincipal> Authorize(OpenIddictRequest request, ClaimsPrincipal principal);
    Task<IActionResult> Logout();
    Task<ClaimsPrincipal> Exchange(OpenIddictRequest request, ClaimsPrincipal principal);
    Task<IActionResult> UserInfo();

    // These are for accepting and denying applications to be authorized
    Task<IActionResult> Accept();
    Task<IActionResult> Deny();
}

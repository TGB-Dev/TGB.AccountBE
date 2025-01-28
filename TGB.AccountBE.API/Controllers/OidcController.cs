using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.UserSessionValidation;

namespace TGB.AccountBE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OidcController : ControllerBase
{
    private readonly IOidcAuthService _authService;

    public OidcController(IOidcAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("[action]")]
    [HttpPost("[action]")]
    [IgnoreAntiforgeryToken]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [UserSessionValidate]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        var principal = User;
        var identity = await _authService.Authorize(request, principal);
        return SignIn(identity,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var res = await _authService.Logout();
        return Ok(res);
    }

    [HttpPost("Token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [UserSessionValidate]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        var principal = User;
        var identity = await _authService.Exchange(request, principal);
        return SignIn(identity, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("[action]")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [UserSessionValidate]
    public async Task<IActionResult> UserInfo()
    {
        var res = await _authService.UserInfo();
        return Ok(res);
    }
}

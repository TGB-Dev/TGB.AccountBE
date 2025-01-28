using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserSessionValidate]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        var res = await _authService.Authorize(request);
        return Ok(res);
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
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        var res = await _authService.Exchange();
        return Ok(res);
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    // [UserSessionValidation.UserSessionValidate]
    [HttpGet("[action]")]
    [Produces("application/json")]
    public async Task<IActionResult> UserInfo()
    {
        var res = await _authService.UserInfo();
        return Ok(res);
    }
}

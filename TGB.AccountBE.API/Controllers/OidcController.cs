using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Exceptions.ErrorExceptions;
using TGB.AccountBE.API.Extensions;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.UserSessionValidation;

namespace TGB.AccountBE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    [Authorize]
    [UserSessionValidate]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new BadRequestErrorException(
                          nameof(HttpErrorResponses.OidcInvalidServerRequest),
                          HttpErrorResponses.OidcInvalidServerRequest);
        var userId = User.GetUserId();
        var identity = await _authService.Authorize(request, userId);
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
    [Authorize]
    [UserSessionValidate]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new BadRequestErrorException(
                          nameof(HttpErrorResponses.OidcInvalidServerRequest),
                          HttpErrorResponses.OidcInvalidServerRequest);
        var principal = User;
        var identity = await _authService.Exchange(request, principal.GetUserId(), principal);
        return SignIn(identity, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("[action]")]
    [Produces("application/json")]
    [Authorize]
    [UserSessionValidate]
    public async Task<IActionResult> UserInfo()
    {
        var userId = User.GetUserId();
        var res = await _authService.UserInfo(userId);
        return Ok(res);
    }
}

using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Client.WebIntegration;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Dtos.Auth;
using TGB.AccountBE.API.Interfaces.Services;

namespace TGB.AccountBE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Register([FromBody] RegisterReqDto body)
    {
        var res = await _authService.Register(body);
        return Ok(res);
    }

    [HttpPost("Login")]
    public async Task<ActionResult<LoginResDto>> Login([FromBody] LoginReqDto body)
    {
        var res = await _authService.Login(body);
        return Ok(res);
    }

    [HttpGet("Login/Google")]
    public ChallengeResult LoginWithGoogle(string returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
        };

        return Challenge(properties, OpenIddictClientWebIntegrationConstants.Providers.Google);
    }

    [HttpGet("Login/GitHub")]
    public ChallengeResult LoginWithGitHub(string returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
        };
        return Challenge(properties, OpenIddictClientWebIntegrationConstants.Providers.GitHub);
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<RefreshTokenResDto>> RefreshToken(
        [FromBody] RefreshTokenReqDto body)
    {
        var res = await _authService.RefreshToken(body);
        return Ok(res);
    }

    [HttpGet("Callback/{provider}")]
    [HttpPost("Callback/{provider}")]
    public async Task<ActionResult<LoginResDto>> ExternalLoginCallback(string provider)
    {
        var result =
            await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults
                .AuthenticationScheme);
        if (!result.Succeeded)
            return Challenge(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

        var displayName = result.Principal.FindFirst(ClaimTypes.Name)!.Value;
        var email = result.Principal.FindFirst(ClaimTypes.Email)!.Value;
        var userNameRandomNumber = new Random().Next(0, 9999);
        var processedDisplayName = DisallowedUserNameCharsRegex()
            .Replace(displayName.Trim().Replace(" ", ""), "");
        var maxUserNameLength = Math.Min(32 - 4, processedDisplayName.Length);
        var userName = processedDisplayName[..maxUserNameLength] +
                       userNameRandomNumber.ToString().PadLeft(4, '0');
        var dateOfBirthClaim = result.Principal.FindFirst(ClaimTypes.DateOfBirth);
        var dateOfBirth = DateTimeOffset.Now;
        if (dateOfBirthClaim != null) DateTimeOffset.Parse(dateOfBirthClaim.Value);

        var res = await _authService.ExternalLogin(new ExternalLoginReqDto
        {
            DisplayName = displayName,
            Email = email,
            UserName = userName,
            DateOfBirth = dateOfBirth
        });

        return Ok(res);
    }

    [GeneratedRegex(UserInfoRules.USERNAME_DISALLOWED_CHARS_PATTERN)]
    private static partial Regex DisallowedUserNameCharsRegex();
}

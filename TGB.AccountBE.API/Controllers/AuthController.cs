using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Client.WebIntegration;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Dtos.Auth;
using TGB.AccountBE.API.Exceptions.ErrorExceptions;
using TGB.AccountBE.API.Interfaces.Services;

namespace TGB.AccountBE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
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
    public ChallengeResult LoginWithGoogle(string returnUri = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.IsLocalUrl(returnUri) ? returnUri : "/"
        };

        return Challenge(properties, OpenIddictClientWebIntegrationConstants.Providers.Google);
    }

    [HttpGet("Login/GitHub")]
    public ChallengeResult LoginWithGitHub(string returnUri = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.IsLocalUrl(returnUri) ? returnUri : "/"
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
        if (!AuthRules.SUPPORTED_EXTERNAL_OAUTH_PROVIDERS.Contains(provider))
            throw new BadRequestErrorException(nameof(HttpErrorResponses.OAuthProviderNotSupported),
                HttpErrorResponses.OAuthProviderNotSupported);

        var result =
            await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults
                .AuthenticationScheme);
        if (!result.Succeeded)
            return Challenge(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

        var userName = "";
        Claim? dateOfBirthClaim = null;
        var dateOfBirth = DateTimeOffset.UtcNow;

        var displayName = result.Principal.FindFirst(ClaimTypes.Name)!.Value;
        var email = result.Principal.FindFirst(ClaimTypes.Email)!.Value;

        switch (provider)
        {
            case "GitHub":
                // Reuse the username from user's GitHub username
                userName = result.Principal.GetClaim("login") ??
                           _authService.GenerateUserNameFromDisplayName(displayName);
                dateOfBirthClaim = result.Principal.FindFirst(ClaimTypes.DateOfBirth);
                break;
            case "Google":
                // Google doesn't provide user's date of birth
                userName = _authService.GenerateUserNameFromDisplayName(displayName);
                break;
        }


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
}

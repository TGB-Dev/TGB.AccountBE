using System.Security.Claims;
using System.Text.RegularExpressions;
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
        if (!AuthRules.SUPPORTED_EXTERNAL_OAUTH_PROVIDERS.Contains(provider))
        {
            throw new BadRequestErrorException("The OAuth provider is not supported",
                "OAuthProviderNotSupported");
        }

        var result =
            await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults
                .AuthenticationScheme);
        if (!result.Succeeded)
            return Challenge(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

        var displayName = "";
        var email = "";
        var userName = "";
        var username = "";
        Claim dateOfBirthClaim = null;
        var dateOfBirth = DateTimeOffset.UtcNow;

        displayName = result.Principal.FindFirst(ClaimTypes.Name)!.Value;
        email = result.Principal.FindFirst(ClaimTypes.Email)!.Value;

        switch (provider)
        {
            case "GitHub":
                // Reuse the username from user's GitHub username
                userName = result.Principal.GetClaim("login");
                dateOfBirthClaim = result.Principal.FindFirst(ClaimTypes.DateOfBirth);
                break;
            case "Google":
                // Google doesn't provide user's date of birth
                userName = _generateRandomUserName(displayName);
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

    [GeneratedRegex(AuthRules.USERNAME_DISALLOWED_CHARS_PATTERN)]
    private static partial Regex DisallowedUserNameCharsRegex();

    private static string _generateRandomUserName(string displayName)
    {
        var randomNumber = new Random().Next(0, 9999);
        // Remove forbidden characters and spaces
        var filteredUserName = DisallowedUserNameCharsRegex()
            .Replace(displayName.Trim().Replace(" ", ""), "");
        var takenLength =
            Math.Min(AuthRules.MAX_USERNAME_LENGTH - AuthRules.USERNAME_RANDOM_PADDING,
                filteredUserName.Length);

        return filteredUserName[..takenLength] +
               randomNumber.ToString().PadLeft(AuthRules.USERNAME_RANDOM_PADDING, '0');
    }
}

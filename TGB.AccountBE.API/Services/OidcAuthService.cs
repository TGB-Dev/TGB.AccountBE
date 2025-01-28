using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Exceptions.ErrorExceptions;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Services;

public class OidcAuthService : IOidcAuthService
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public OidcAuthService(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> Accept()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> Authorize(OpenIddictRequest request)
    {
        // Currently we directly accept the request because:
        // 1. We don't accept external client applications' requests
        // 2. If needed in the future, the accept/deny functionality will be handled by the front-end
        // team

        // If this function is called, the user is authorized to do this, so we don't need to check
        // that, and proceed to the client application authorization

        if (request.ClientId == null)
        {
            throw new BadRequestErrorException(nameof(HttpErrorResponses.OAuthClientIdNotProvided),
                HttpErrorResponses.OAuthClientIdNotProvided);
        }

        var application = await _applicationManager.FindByClientIdAsync(
            request.ClientId
        );

        throw new NotImplementedException();
    }

    public async Task<IActionResult> Deny()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> Exchange()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> Logout()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> UserInfo()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> Login()
    {
        throw new NotImplementedException();
    }
}

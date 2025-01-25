using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
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

    public async Task<IActionResult> Authorize()
    {
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

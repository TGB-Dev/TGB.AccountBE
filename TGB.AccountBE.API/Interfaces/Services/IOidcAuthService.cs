using Microsoft.AspNetCore.Mvc;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IOidcAuthService
{
    Task<IActionResult> Authorize();
    Task<IActionResult> Logout();
    Task<IActionResult> Exchange();
    Task<IActionResult> UserInfo();

    // These are for accepting and denying applications to be authorized
    Task<IActionResult> Accept();
    Task<IActionResult> Deny();
}

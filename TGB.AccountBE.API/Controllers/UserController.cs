using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TGB.AccountBE.API.Dtos.User;
using TGB.AccountBE.API.Extensions;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.UserSessionValidation;

namespace TGB.AccountBE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [UserSessionValidate]
    [HttpGet("[action]")]
    public async Task<ActionResult<MeResDto>> Me()
    {
        var userId = User.GetUserId();

        var res = await _userService.Me(userId);
        return Ok(res);
    }
}

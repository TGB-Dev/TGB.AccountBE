using Microsoft.AspNetCore.Mvc;
using TGB.AccountBE.API.Dtos.Auth;
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

    [HttpPost("[action]")]
    public async Task<ActionResult<LoginResDto>> Login([FromBody] LoginReqDto body)
    {
        var res = await _authService.Login(body);
        return Ok(res);
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<RefreshTokenResDto>> RefreshToken(
        [FromBody] RefreshTokenReqDto body)
    {
        var res = await _authService.RefreshToken(body);
        return Ok(res);
    }
}

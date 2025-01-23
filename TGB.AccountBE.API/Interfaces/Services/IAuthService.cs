using TGB.AccountBE.API.Dtos.Auth;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IAuthService
{
    Task<RegisterResDto> Register(RegisterReqDto dto);
    Task<LoginResDto> Login(LoginReqDto dto);
    Task<RefreshTokenResDto> RefreshToken(RefreshTokenReqDto dto);
}

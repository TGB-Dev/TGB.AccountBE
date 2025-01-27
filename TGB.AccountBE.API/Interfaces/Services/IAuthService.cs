using TGB.AccountBE.API.Dtos.Auth;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IAuthService
{
    Task<RegisterResDto> Register(RegisterReqDto dto, bool isEmailConfirmed = false);
    Task<LoginResDto> Login(LoginReqDto dto);
    Task<RefreshTokenResDto> RefreshToken(RefreshTokenReqDto dto);
    Task<LoginResDto> ExternalLogin(ExternalLoginReqDto dto);
    string GenerateUserNameFromDisplayName(string displayName);
}

using Microsoft.AspNetCore.Identity;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Dtos.Auth;
using TGB.AccountBE.API.Exceptions.ErrorExceptions;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Services;

public class AuthService : IAuthService
{
    private readonly IRandomPasswordGenerator _randomPasswordGenerator;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserSessionService _userSessionService;

    public AuthService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserSessionService userSessionService,
        IRandomPasswordGenerator randomPasswordGenerator)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userSessionService = userSessionService;
        _randomPasswordGenerator = randomPasswordGenerator;
    }

    public async Task<RegisterResDto> Register(RegisterReqDto dto, bool isEmailConfirmed = false)
    {
        if (await _userManager.FindByNameAsync(dto.UserName) != null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.UserNameAlreadyExists),
                HttpErrorResponses.UserNameAlreadyExists);

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.EmailAlreadyExists),
                HttpErrorResponses.EmailAlreadyExists);

        var user = new ApplicationUser
        {
            DisplayName = dto.DisplayName,
            UserName = dto.UserName,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth.UtcDateTime,
            EmailConfirmed = isEmailConfirmed
        };

        var createdUser = await _userManager.CreateAsync(user, dto.Password);

        if (!createdUser.Succeeded)
            throw new Exception(
                string.Join(", ", createdUser.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.User);
        if (!roleResult.Succeeded)
            throw new Exception(
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        return new RegisterResDto
        {
            Succeeded = true,
            Message = "User created successfully"
        };
    }

    public async Task<LoginResDto> Login(LoginReqDto dto)
    {
        // Console.WriteLine($"Login: {dto.UserName}, {dto.Password}");
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
            throw new BadRequestErrorException(
                nameof(HttpErrorResponses.InvalidUserNameOrPassword),
                HttpErrorResponses.InvalidUserNameOrPassword);

        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new BadRequestErrorException(
                nameof(HttpErrorResponses.EmailNotConfirmed),
                HttpErrorResponses.EmailNotConfirmed);

        var checkPasswordResult =
            await _signInManager.CheckPasswordSignInAsync(user, dto.Password, true);

        if (!checkPasswordResult.Succeeded)
            throw new BadRequestErrorException(
                nameof(HttpErrorResponses.InvalidUserNameOrPassword),
                HttpErrorResponses.InvalidUserNameOrPassword);

        var userSession = await _userSessionService.CreateUserSession(user);

        return new LoginResDto
        {
            AccessToken = userSession.AccessToken,
            RefreshToken = userSession.RefreshToken
        };
    }

    public async Task<RefreshTokenResDto> RefreshToken(RefreshTokenReqDto dto)
    {
        var userSession = await _userSessionService.UpdateUserSession(dto.RefreshToken);

        return new RefreshTokenResDto
        {
            AccessToken = userSession.AccessToken,
            RefreshToken = userSession.RefreshToken
        };
    }

    public async Task<LoginResDto> ExternalLogin(ExternalLoginReqDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        // We determine if the user is already exist using email address
        if (user != null)
        {
            // Update (but not overwrite) information from external provider
            user.DisplayName ??= dto.DisplayName;
            user.UserName ??= dto.UserName;
            user.Email ??= dto.Email;
            user.DateOfBirth ??= dto.DateOfBirth.UtcDateTime;
        }
        else
        {
            // The providers that we're using here requires email verification, so we don't need to
            // do that anymore
            var result = await Register(new RegisterReqDto
            {
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                UserName = dto.UserName,
                DateOfBirth = dto.DateOfBirth.UtcDateTime,
                Password = _randomPasswordGenerator.Generate()
            }, true);

            if (!result.Succeeded)
                throw new Exception($"Error while registering user via OIDC: {result.Message}");

            user = await _userManager.FindByEmailAsync(dto.Email);
        }

        if (user == null)
            throw new InvalidDataException("The created user from OAuth service is null");

        var userSession = await _userSessionService.CreateUserSession(user);

        return new LoginResDto
        {
            AccessToken = userSession.AccessToken,
            RefreshToken = userSession.RefreshToken
        };
    }
}

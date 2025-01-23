using Microsoft.AspNetCore.Identity;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Dtos.User;
using TGB.AccountBE.API.Exceptions.ErrorExceptions;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<MeResDto> Me(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.UserNotFound),
                HttpErrorResponses.UserNotFound);

        return new MeResDto
        {
            Email = user.Email,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            DateOfBirth = user.DateOfBirth
        };
    }
}

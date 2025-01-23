using TGB.AccountBE.API.Dtos.User;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IUserService
{
    Task<MeResDto> Me(string userId);
}

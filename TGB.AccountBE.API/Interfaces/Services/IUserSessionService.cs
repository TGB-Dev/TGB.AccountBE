using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IUserSessionService
{
    Task<UserSessionSql> CreateUserSession(ApplicationUser user);

    Task DeleteUserSession(string userSessionId);

    Task<UserSessionSql> UpdateUserSession(string refreshToken);
}

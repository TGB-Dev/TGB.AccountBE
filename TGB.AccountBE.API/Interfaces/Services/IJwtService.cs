using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user);
}

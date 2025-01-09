using TGB.AccountBE.API.Database;

namespace TGB.AccountBE.API.Interfaces;

public interface IJwtService
{
    public string GenerateJwtToken(ApplicationUser user);
}

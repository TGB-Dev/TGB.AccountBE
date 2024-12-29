using Microsoft.AspNetCore.Identity;

namespace TGB.AccountBE.API.Database;

public class ApplicationUser: IdentityUser
{
    [ProtectedPersonalData] public string? NationalId { get; set; }
}

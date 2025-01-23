using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TGB.AccountBE.API.Models.Sql;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [Column(TypeName = "date")]
    public DateTime? DateOfBirth { get; set; }

    [PersonalData] public string? DisplayName { get; set; }
    [ProtectedPersonalData] public string? NationalId { get; set; }
    public virtual ICollection<UserSessionSql>? UserSessions { get; set; }
}

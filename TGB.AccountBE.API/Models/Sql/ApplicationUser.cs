using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TGB.AccountBE.API.Models.Sql;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [Column(TypeName = "date")]
    public DateTime? DateOfBirth { get; set; }

    [PersonalData] [MaxLength(30)] public string? DisplayName { get; set; }
    [ProtectedPersonalData] [MaxLength(12)] public string? NationalId { get; set; }
    [PersonalData] [MaxLength(30)] public string? GoogleSub { get; set; }
    [PersonalData] [MaxLength(30)] public string? GitHubSub { get; set; }
    public virtual ICollection<UserSessionSql>? UserSessions { get; set; }
}

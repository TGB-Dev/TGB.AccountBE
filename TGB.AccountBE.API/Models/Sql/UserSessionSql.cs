using System.ComponentModel.DataAnnotations;

namespace TGB.AccountBE.API.Models.Sql;

public class UserSessionSql
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required] public virtual required ApplicationUser User { get; set; }

    [Required] public required string AccessToken { get; set; }
    [Required] public required string RefreshToken { get; set; }
    [Required] public required DateTime RefreshTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

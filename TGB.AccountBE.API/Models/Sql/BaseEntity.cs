using System.ComponentModel.DataAnnotations;

namespace TGB.AccountBE.API.Models.Sql;

public class BaseEntity
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

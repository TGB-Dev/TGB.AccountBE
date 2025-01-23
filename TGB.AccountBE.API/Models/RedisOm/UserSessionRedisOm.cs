using Redis.OM.Modeling;

namespace TGB.AccountBE.API.Models.RedisOm;

[Document(StorageType = StorageType.Hash, Prefixes = ["UserSession"])]
public class UserSessionRedisOm
{
    [Indexed] [RedisIdField] public required string Id { get; set; }
    [Indexed] public required string UserId { get; set; }
    [Indexed] public required string AccessToken { get; set; }
    [Indexed] public required string RefreshToken { get; set; }
    [Indexed] public required DateTime RefreshTokenExpiresAt { get; set; }
    [Indexed] public required DateTime CreatedAt { get; set; }
    [Indexed] public required DateTime UpdatedAt { get; set; }
}

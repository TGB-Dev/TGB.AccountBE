using TGB.AccountBE.API.Models.RedisOm;

namespace TGB.AccountBE.API.Interfaces.Repository.RedisOm;

public interface IUserSessionRepositoryRedisOm : IRepositoryBaseRedisOm<UserSessionRedisOm>
{
    Task<UserSessionRedisOm?> GetByAccessToken(string accessToken);
    Task<UserSessionRedisOm?> GetByRefreshToken(string refreshToken);
    Task<IList<UserSessionRedisOm>> GetByUserId(string id);
}

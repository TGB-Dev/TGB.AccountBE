using Redis.OM;
using TGB.AccountBE.API.Interfaces.Repository.RedisOm;
using TGB.AccountBE.API.Models.RedisOm;

namespace TGB.AccountBE.API.Repository.RedisOm;

public class UserSessionRepositoryRedisOm : RepositoryBaseRedisOm<UserSessionRedisOm>,
    IUserSessionRepositoryRedisOm
{
    public UserSessionRepositoryRedisOm(RedisConnectionProvider provider) : base(provider)
    {
    }

    public async Task<UserSessionRedisOm?> GetByAccessToken(string accessToken)
    {
        return await Collection.Where(u => u.AccessToken == accessToken).FirstOrDefaultAsync();
    }

    public async Task<UserSessionRedisOm?> GetByRefreshToken(string refreshToken)
    {
        return await Collection.Where(u => u.RefreshToken == refreshToken).FirstOrDefaultAsync();
    }

    public Task<IList<UserSessionRedisOm>> GetByUserId(string id)
    {
        return Collection.Where(u => u.UserId == id).ToListAsync();
    }
}

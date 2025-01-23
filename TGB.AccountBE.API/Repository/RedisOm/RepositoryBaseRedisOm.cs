using Redis.OM;
using Redis.OM.Searching;
using TGB.AccountBE.API.Interfaces.Repository.RedisOm;

namespace TGB.AccountBE.API.Repository.RedisOm;

public class RepositoryBaseRedisOm<T> : IRepositoryBaseRedisOm<T> where T : class
{
    protected readonly IRedisCollection<T> Collection;

    public RepositoryBaseRedisOm(RedisConnectionProvider provider)
    {
        Collection = provider.RedisCollection<T>();
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await Collection.FindByIdAsync(id);
    }

    public async Task InsertAsync(T entity)
    {
        await Collection.InsertAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        await Collection.UpdateAsync(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        await Collection.DeleteAsync(entity);
    }

    public async Task<IList<T>> GetAllAsync()
    {
        return await Collection.ToListAsync();
    }
}

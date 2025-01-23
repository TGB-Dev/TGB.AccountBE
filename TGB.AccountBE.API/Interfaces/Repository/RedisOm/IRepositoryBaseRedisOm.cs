namespace TGB.AccountBE.API.Interfaces.Repository.RedisOm;

public interface IRepositoryBaseRedisOm<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IList<T>> GetAllAsync();
    Task InsertAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

namespace TGB.AccountBE.API.Interfaces.Repository.Sql;

public interface IRepositoryBaseSql<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<T?> DeleteAsync(string id);
    Task<T> DeleteAsync(T entity);
}

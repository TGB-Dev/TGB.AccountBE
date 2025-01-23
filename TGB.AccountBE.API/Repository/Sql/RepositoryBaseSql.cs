using Microsoft.EntityFrameworkCore;
using TGB.AccountBE.API.Database;
using TGB.AccountBE.API.Interfaces.Repository.Sql;

namespace TGB.AccountBE.API.Repository.Sql;

public class RepositoryBaseSql<T> : IRepositoryBaseSql<T> where T : class
{
    protected readonly ApplicationDbContext Context;

    public RepositoryBaseSql(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<IList<T>> GetAllAsync()
    {
        return await Context.Set<T>().ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return null;

        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await Context.Set<T>().FindAsync(id);
    }

    public async Task<T> DeleteAsync(T entity)
    {
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
        return entity;
    }
}

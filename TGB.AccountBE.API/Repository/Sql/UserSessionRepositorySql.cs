using Microsoft.EntityFrameworkCore;
using TGB.AccountBE.API.Database;
using TGB.AccountBE.API.Interfaces.Repository.Sql;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Repository.Sql;

public class UserSessionRepositorySql : RepositoryBaseSql<UserSessionSql>, IUserSessionRepositorySql
{
    public UserSessionRepositorySql(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserSessionSql?> GetByAccessToken(string accessToken)
    {
        return await Context.UserSessions.FirstOrDefaultAsync(u => u.AccessToken == accessToken);
    }

    public async Task<UserSessionSql?> GetByRefreshToken(string refreshToken)
    {
        return await Context.UserSessions.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<IList<UserSessionSql>> GetByUserId(string id)
    {
        return await Context.UserSessions.Where(u => u.User.Id == id).ToListAsync();
    }
}

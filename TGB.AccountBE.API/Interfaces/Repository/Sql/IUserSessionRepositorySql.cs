using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Interfaces.Repository.Sql;

public interface IUserSessionRepositorySql : IRepositoryBaseSql<UserSessionSql>
{
    Task<UserSessionSql?> GetByAccessToken(string accessToken);
    Task<UserSessionSql?> GetByRefreshToken(string refreshToken);
    Task<IList<UserSessionSql>> GetByUserId(string id);
}

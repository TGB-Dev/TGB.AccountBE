using Microsoft.AspNetCore.Mvc.Filters;
using TGB.AccountBE.API.Interfaces.Repository.RedisOm;
using TGB.AccountBE.API.Interfaces.Repository.Sql;

namespace TGB.AccountBE.API.UserSessionValidation;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class UserSessionValidateAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var userSessionRepositorySql =
            serviceProvider.GetRequiredService<IUserSessionRepositorySql>();
        var userSessionRepositoryRedisOm =
            serviceProvider.GetRequiredService<IUserSessionRepositoryRedisOm>();

        return new UserSessionValidateActionFilter(userSessionRepositorySql,
            userSessionRepositoryRedisOm);
    }
}

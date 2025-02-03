using Microsoft.AspNetCore.Mvc.Filters;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Exceptions.ErrorExceptions;
using TGB.AccountBE.API.Interfaces.Repository.RedisOm;
using TGB.AccountBE.API.Interfaces.Repository.Sql;
using TGB.AccountBE.API.Models.RedisOm;

namespace TGB.AccountBE.API.UserSessionValidation;

public class UserSessionValidateActionFilter : IAsyncActionFilter
{
    private readonly IUserSessionRepositoryRedisOm _userSessionRepositoryRedisOm;
    private readonly IUserSessionRepositorySql _userSessionRepositorySql;

    public UserSessionValidateActionFilter(IUserSessionRepositorySql userSessionRepositorySql,
        IUserSessionRepositoryRedisOm userSessionRepositoryRedisOm)
    {
        _userSessionRepositorySql = userSessionRepositorySql;
        _userSessionRepositoryRedisOm = userSessionRepositoryRedisOm;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var token = context.HttpContext.Request.Headers.Authorization.ToString()
            .Replace("Bearer ", "");

        var userSessionRedisOmResult = await _userSessionRepositoryRedisOm.GetByAccessToken(token);
        if (userSessionRedisOmResult is not null)
        {
            await next();
            return;
        }

        var userSessionSqlResult = await _userSessionRepositorySql.GetByAccessToken(token);
        if (userSessionSqlResult is null)
            throw new UnauthorizedErrorException(nameof(HttpErrorResponses.InvalidAccessToken),
                HttpErrorResponses.InvalidAccessToken);

        var userSessionRedisOm = new UserSessionRedisOm
        {
            Id = userSessionSqlResult.Id,
            UserId = userSessionSqlResult.User.Id,
            AccessToken = userSessionSqlResult.AccessToken,
            RefreshToken = userSessionSqlResult.RefreshToken,
            RefreshTokenExpiresAt = userSessionSqlResult.RefreshTokenExpiresAt,
            CreatedAt = userSessionSqlResult.CreatedAt,
            UpdatedAt = userSessionSqlResult.UpdatedAt
        };
        await _userSessionRepositoryRedisOm.InsertAsync(userSessionRedisOm);

        await next();
    }
}

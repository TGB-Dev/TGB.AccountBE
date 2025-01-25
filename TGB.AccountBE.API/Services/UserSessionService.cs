using System.Security.Cryptography;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Exceptions.ErrorExceptions;
using TGB.AccountBE.API.Interfaces.Repository.RedisOm;
using TGB.AccountBE.API.Interfaces.Repository.Sql;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.RedisOm;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Services;

public class UserSessionService : IUserSessionService
{
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;
    private readonly IUserSessionRepositoryRedisOm _userSessionRepositoryRedisOm;
    private readonly IUserSessionRepositorySql _userSessionRepositorySql;

    public UserSessionService(IConfiguration configuration,
        IJwtService jwtService,
        IUserSessionRepositorySql userSessionRepositorySql,
        IUserSessionRepositoryRedisOm userSessionRepositoryRedisOm)
    {
        _configuration = configuration;
        _jwtService = jwtService;
        _userSessionRepositorySql = userSessionRepositorySql;
        _userSessionRepositoryRedisOm = userSessionRepositoryRedisOm;
    }

    public async Task DeleteUserSession(string userSessionId)
    {
        var userSessionRedisOm = await _userSessionRepositoryRedisOm.GetByIdAsync(userSessionId);
        var userSessionSql = await _userSessionRepositorySql.GetByIdAsync(userSessionId);

        if (userSessionRedisOm != null)
            await _userSessionRepositoryRedisOm.DeleteAsync(userSessionRedisOm);

        if (userSessionSql == null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.UserSessionNotFound),
                HttpErrorResponses.UserSessionNotFound);

        await _userSessionRepositorySql.DeleteAsync(userSessionSql);
    }

    public async Task<UserSessionSql> CreateUserSession(ApplicationUser user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var userSessionSql = await CreateUserSessionSql(user, accessToken, refreshToken);
        await CreateUserSessionRedisOm(userSessionSql);

        return userSessionSql;
    }

    public async Task<UserSessionSql> UpdateUserSession(string refreshToken)
    {
        var userSessionSql = await _userSessionRepositorySql.GetByRefreshToken(refreshToken);

        if (userSessionSql == null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.UserSessionNotFound),
                HttpErrorResponses.UserSessionNotFound);

        if (DateTime.UtcNow > userSessionSql.RefreshTokenExpiresAt)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.RefreshTokenExpired),
                HttpErrorResponses.RefreshTokenExpired);

        var newAccessToken = _jwtService.GenerateAccessToken(userSessionSql.User);
        var newRefreshToken = GenerateRefreshToken();

        var userSessionSqlNew =
            await UpdateUserSessionSql(userSessionSql, newAccessToken, newRefreshToken);

        var userSessionRedisOm =
            await _userSessionRepositoryRedisOm.GetByIdAsync(userSessionSqlNew.Id);

        if (userSessionRedisOm == null)
            await CreateUserSessionRedisOm(userSessionSqlNew);
        else
            await UpdateUserSessionRedisOm(userSessionSqlNew, userSessionRedisOm);
        return userSessionSqlNew;
    }

    private async Task<UserSessionSql> CreateUserSessionSql(ApplicationUser user,
        string accessToken, string refreshToken)
    {
        var userSessionSql = new UserSessionSql
        {
            User = user,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt =
                DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Token:RefreshToken:ExpiresInMinutes"]!))
        };

        return await _userSessionRepositorySql.AddAsync(userSessionSql);
    }

    private async Task CreateUserSessionRedisOm(UserSessionSql userSessionSql)
    {
        var userSessionRedisOm = new UserSessionRedisOm
        {
            Id = userSessionSql.Id,
            UserId = userSessionSql.User.Id,
            AccessToken = userSessionSql.AccessToken,
            RefreshToken = userSessionSql.RefreshToken,
            RefreshTokenExpiresAt = userSessionSql.RefreshTokenExpiresAt,
            CreatedAt = userSessionSql.CreatedAt,
            UpdatedAt = userSessionSql.UpdatedAt
        };

        await _userSessionRepositoryRedisOm.InsertAsync(userSessionRedisOm);
    }

    private async Task<UserSessionSql> UpdateUserSessionSql(UserSessionSql userSessionSql,
        string newAccessToken,
        string newRefreshToken)
    {
        userSessionSql.AccessToken = newAccessToken;
        userSessionSql.RefreshToken = newRefreshToken;
        userSessionSql.RefreshTokenExpiresAt =
            DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Token:RefreshToken:ExpiresInMinutes"]!));

        return await _userSessionRepositorySql.UpdateAsync(userSessionSql);
    }

    private async Task UpdateUserSessionRedisOm(UserSessionSql userSessionSql,
        UserSessionRedisOm userSessionRedisOm)
    {
        userSessionRedisOm.AccessToken = userSessionSql.AccessToken;
        userSessionRedisOm.RefreshToken = userSessionSql.RefreshToken;
        userSessionRedisOm.RefreshTokenExpiresAt = userSessionSql.RefreshTokenExpiresAt;
        userSessionRedisOm.UpdatedAt = userSessionSql.UpdatedAt;

        await _userSessionRepositoryRedisOm.UpdateAsync(userSessionRedisOm);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}

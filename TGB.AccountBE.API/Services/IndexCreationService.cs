using Redis.OM;
using TGB.AccountBE.API.Models.RedisOm;

namespace TGB.AccountBE.API.Services;

public class IndexCreationService : IHostedService
{
    private readonly ILogger _logger;
    private readonly RedisConnectionProvider _provider;

    public IndexCreationService(ILogger<IndexCreationService> logger,
        RedisConnectionProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating indexes...");
        await _provider.Connection.CreateIndexAsync(typeof(UserSessionRedisOm));
        _logger.LogInformation("Indexes created.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

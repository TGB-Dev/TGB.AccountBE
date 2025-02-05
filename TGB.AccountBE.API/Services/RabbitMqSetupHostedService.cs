using RabbitMQ.Client;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Services;

public class RabbitMqSetupHostedService : IHostedService
{
    private readonly IConnection _connection;

    public RabbitMqSetupHostedService(IConnection connection)
    {
        _connection = connection;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var queueList = new List<string>
        {
            RabbitMqInfo.EmailQueue
        };

        foreach (var queueName in queueList)
        {
            await using var channel =
                await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            var res = await channel.QueueDeclareAsync(
                queueName,
                true,
                false,
                false,
                null,
                cancellationToken: cancellationToken
            );

            if (!res.QueueName.Equals(queueName))
                throw new Exception($"Failed to create queue {queueName}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

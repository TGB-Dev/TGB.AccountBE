using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Services;

public class EmailService : IEmailService
{
    private readonly IConnection _connection;

    public EmailService(IConnection connection)
    {
        _connection = connection;
    }

    public async Task SendAccountVerificationEmailAsync(ApplicationUser user,
        string verificationToken)
    {
        var body = new
        {
            user.Email,
            user.DisplayName,
            user.UserName,
            VerificationToken = verificationToken,
            Type = EmailType.AccountVerification
        };

        await PublishMessageAsync(body);
    }

    public async Task SendPasswordResetEmailAsync(ApplicationUser user, string resetToken)
    {
        var body = new
        {
            user.Email,
            user.DisplayName,
            user.UserName,
            VerificationToken = resetToken,
            Type = EmailType.PasswordReset
        };

        await PublishMessageAsync(body);
    }

    public async Task SendPasswordChangedEmailAsync(ApplicationUser user)
    {
        var body = new
        {
            user.Email,
            user.DisplayName,
            user.UserName,
            Type = EmailType.PasswordChanged
        };

        await PublishMessageAsync(body);
    }

    private async Task PublishMessageAsync(object message)
    {
        await using var channel = await _connection.CreateChannelAsync();

        var body = JsonSerializer.Serialize(message);
        var bytesBody = Encoding.UTF8.GetBytes(body);

        await channel.BasicPublishAsync(
            RabbitMqInfo.Exchange,
            RabbitMqInfo.EmailQueue,
            bytesBody
        );
    }
}

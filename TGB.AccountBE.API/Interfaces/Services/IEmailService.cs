using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Interfaces.Services;

public interface IEmailService
{
    Task SendAccountVerificationEmailAsync(ApplicationUser user, string verificationToken);
    Task SendPasswordResetEmailAsync(ApplicationUser user, string resetToken);
    Task SendPasswordChangedEmailAsync(ApplicationUser user);
}

using OpenIddict.Abstractions;
using TGB.AccountBE.API.Database;

namespace TGB.AccountBE.API;

public class ApplicationRegisterWorkerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationRegisterWorkerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var applicationDescriptors = new List<OpenIddictApplicationDescriptor>
        {
            // This test application is intended for use in Postman tests only
            new()
            {
                ClientId = "TGB.ContestPlatformBE",
                ClientSecret = "test",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                DisplayName = "TGB Contest Platform Backend",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles
                },

                RedirectUris =
                {
                    new Uri("https://localhost:6868/api/Login/Callback/Local")
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            }
        };
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var applicationManager =
            scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        foreach (var applicationDescriptor in applicationDescriptors)
            if (await applicationManager.FindByClientIdAsync(applicationDescriptor.ClientId!,
                    cancellationToken) is null)
                await applicationManager.CreateAsync(applicationDescriptor, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

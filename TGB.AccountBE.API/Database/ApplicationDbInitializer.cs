using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Database;

public class ApplicationDbInitializer : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationDbInitializer> _logger;

    public ApplicationDbInitializer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<ApplicationDbInitializer> logger
        )
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Seeding database
        await RolesSeedAsync(roleManager);
        await UsersSeedAsync(userManager);

        // Create trigger function and trigger for each table
        await CreateTriggerFunctionAsync(context);
        await CreateTriggerForTableAsync(context);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task CreateTriggerFunctionAsync(ApplicationDbContext context)
    {
        const string createFunctionSql = """
                                         CREATE OR REPLACE FUNCTION set_updated_at()
                                         RETURNS TRIGGER AS $$
                                         BEGIN
                                             NEW."UpdatedAt" = NOW();
                                             RETURN NEW;
                                         END;
                                         $$ LANGUAGE plpgsql;
                                         """;
        try
        {
            await context.Database.ExecuteSqlRawAsync(createFunctionSql);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create trigger function");
        }
    }

    private async Task CreateTriggerForTableAsync(ApplicationDbContext context)
    {
        var entityTypes = context.Model.GetEntityTypes();

        foreach (var entity in entityTypes)
        {
            if (entity.FindProperty("UpdatedAt") is null)
            {
                continue;
            }
            
            var table = entity.GetTableName();
            if (string.IsNullOrWhiteSpace(table))
                continue;
            
            var sql = $"""
                       DO $$
                       BEGIN
                           IF EXISTS (
                               SELECT 1 
                               FROM pg_trigger t
                               JOIN pg_class c ON c.oid = t.tgrelid
                               WHERE c.relname = '{table}' 
                               AND t.tgname = 'set_updated_at_trigger'
                           ) THEN
                               EXECUTE 'DROP TRIGGER set_updated_at_trigger ON "{table}"';
                           END IF;
                           
                           EXECUTE 'CREATE TRIGGER set_updated_at_trigger
                                    BEFORE UPDATE ON "{table}"
                                    FOR EACH ROW
                                    EXECUTE PROCEDURE set_updated_at()';
                       END
                       $$ LANGUAGE plpgsql;
                       """;
            try
            {
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create trigger for table {Table}", table);
            }
        }
    }

    private async Task RolesSeedAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new List<IdentityRole>
        {
            new() { Name = Roles.Admin, NormalizedName = Roles.Admin.ToUpper() },
            new() { Name = Roles.Moderator, NormalizedName = Roles.Moderator.ToUpper() },
            new() { Name = Roles.User, NormalizedName = Roles.User.ToUpper() }
        };

        foreach (var role in roles)
        {
            if (role.Name is not null && await roleManager.RoleExistsAsync(role.Name))
                continue;

            var roleResult = await roleManager.CreateAsync(role);
            if (roleResult.Succeeded) continue;
            
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to create role {role.Name}: {errors}");
        }
    }

    private async Task UsersSeedAsync(UserManager<ApplicationUser> userManager)
    {
        var adminUser = new ApplicationUser
        {
            UserName = _configuration["Admin:Username"]!,
            Email = _configuration["Admin:Email"]!,
            EmailConfirmed = true,
            DisplayName = _configuration["Admin:DisplayName"]!
        };

        var adminPassword = _configuration["Admin:Password"]!;

        if (await userManager.FindByNameAsync(adminUser.UserName) is not null)
            return;

        var userResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!userResult.Succeeded)
        {
            var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to create admin user: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        if (!roleResult.Succeeded)
        { 
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to add role {Roles.Admin} to admin user: {errors}");
        }
    }
}

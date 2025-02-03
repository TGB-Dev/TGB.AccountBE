using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Database;

public class ApplicationDbInitializer : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationDbInitializer(
        IConfiguration configuration,
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager
    )
    {
        _configuration = configuration;
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Seeding database
        await RolesSeedAsync();
        await UsersSeedAsync();

        // Create trigger function and trigger for each table
        await CreateTriggerFunctionAsync();
        await CreateTriggerForTableAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task CreateTriggerFunctionAsync()
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

        await _context.Database.ExecuteSqlRawAsync(createFunctionSql);
    }

    private async Task CreateTriggerForTableAsync()
    {
        var tables = _context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Distinct()
            .ToList();

        foreach (var sql in tables.Select(table => $"""
                                                    DO $$
                                                    BEGIN
                                                        IF EXISTS (SELECT 1 
                                                                   FROM pg_trigger t
                                                                   JOIN pg_class c ON c.oid = t.tgrelid
                                                                   WHERE c.relname = '{table}' 
                                                                   AND t.tgname = 'set_updated_at_trigger') THEN
                                                            EXECUTE 'DROP TRIGGER set_updated_at_trigger ON {table}';
                                                        END IF;
                                                    
                                                        EXECUTE 'CREATE TRIGGER set_updated_at_trigger 
                                                                 BEFORE UPDATE ON {table}
                                                                 FOR EACH ROW 
                                                                 EXECUTE PROCEDURE set_updated_at()';
                                                    END $$;
                                                    """))
            await _context.Database.ExecuteSqlRawAsync(sql);
    }

    private async Task RolesSeedAsync()
    {
        var roles = new List<IdentityRole>
        {
            new() { Name = Roles.Admin, NormalizedName = Roles.Admin.ToUpper() },
            new() { Name = Roles.Moderator, NormalizedName = Roles.Moderator.ToUpper() },
            new() { Name = Roles.User, NormalizedName = Roles.User.ToUpper() }
        };

        foreach (var role in roles)
        {
            if (role.Name is not null && await _roleManager.RoleExistsAsync(role.Name))
                continue;

            var roleResult = await _roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
                throw new Exception("Failed to create roles");
        }
    }

    private async Task UsersSeedAsync()
    {
        var adminUser = new ApplicationUser
        {
            UserName = _configuration["Admin:Username"]!,
            Email = _configuration["Admin:Email"]!,
            EmailConfirmed = true,
            DisplayName = _configuration["Admin:DisplayName"]!
        };

        var adminPassword = _configuration["Admin:Password"]!;

        if (await _userManager.FindByNameAsync(adminUser.UserName) is not null)
            return;

        var userResult = await _userManager.CreateAsync(adminUser, adminPassword);
        if (!userResult.Succeeded)
            throw new Exception("Failed to create admin user");

        var roleResult = await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
        if (!roleResult.Succeeded)
            throw new Exception("Failed to add admin role to admin user");
    }
}

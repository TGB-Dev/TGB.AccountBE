using Microsoft.AspNetCore.Identity;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Database;

public static class ApplicationDbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            await RolesSeedAsync(roleManager);
            await UsersSeedAsync(userManager, configuration);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task RolesSeedAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new List<IdentityRole>
        {
            new() { Name = Roles.Admin, NormalizedName = Roles.Admin.ToUpper() },
            new() { Name = Roles.Moderator, NormalizedName = Roles.Moderator.ToUpper() },
            new() { Name = Roles.User, NormalizedName = Roles.User.ToUpper() }
        };

        foreach (var role in roles)
        {
            if (role.Name != null && await roleManager.RoleExistsAsync(role.Name))
                continue;

            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
                throw new Exception("Failed to create roles");
        }
    }

    private static async Task UsersSeedAsync(UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var adminUser = new ApplicationUser
        {
            UserName = configuration["Admin:Username"] ?? "admin",
            Email = configuration["Admin:Email"] ?? "emailofadmin@email.com",
            EmailConfirmed = true,
            DisplayName = configuration["Admin:DisplayName"] ?? "Admin"
        };

        var adminPassword = configuration["Admin:Password"] ?? "AdminPassword123@";

        if (await userManager.FindByNameAsync(adminUser.UserName) != null)
            return;

        var userResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!userResult.Succeeded)
            throw new Exception("Failed to create admin user");

        var roleResult = await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        if (!roleResult.Succeeded)
            throw new Exception("Failed to add admin role to admin user");
    }
}

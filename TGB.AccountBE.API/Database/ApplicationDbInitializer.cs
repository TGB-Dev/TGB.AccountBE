using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TGB.AccountBE.API.Database;

public class ApplicationDbInitializer
{
    private static void RolesSeed(DbContext context)
    {
        var roles = new List<IdentityRole>
        {
            new()
            {
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new()
            {
                Name = "Moderator",
                NormalizedName = "MODERATOR"
            },
            new()
            {
                Name = "User",
                NormalizedName = "USER"
            }
        };

        roles.ForEach(role =>
        {
            var existingRole =
                context.Set<IdentityRole>().FirstOrDefault(
                    r => r.NormalizedName == role.NormalizedName);

            if (existingRole != null)
                return;

            context.Set<IdentityRole>().Add(role);
            context.SaveChanges();
        });
    }

    public static void Seed(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((context, _) => { RolesSeed(context); });
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Database;

public class ApplicationDbInitializer
{
    private static void RolesSeed(DbContext context)
    {
        var roles = new List<IdentityRole>();
        Enum.GetValues<Roles>().ToList().ForEach(role =>
        {
            roles.Add(new IdentityRole
            {
                Name = role.ToString(),
                NormalizedName = role.ToString().ToUpper()
            });
        });

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

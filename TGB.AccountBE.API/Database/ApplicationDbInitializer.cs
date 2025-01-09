using Microsoft.EntityFrameworkCore;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Database;

public static class ApplicationDbInitializer
{
    private static void RolesSeed(DbContext context)
    {
        var roles = new List<ApplicationRole>();
        Enum.GetValues<Roles>().ToList().ForEach(role =>
        {
            roles.Add(new ApplicationRole
            {
                Name = role.ToString(),
                NormalizedName = role.ToString().ToUpper()
            });
        });

        roles.ForEach(role =>
        {
            var existingRole =
                context.Set<ApplicationRole>().FirstOrDefault(
                    r => r.NormalizedName == role.NormalizedName);

            if (existingRole != null)
                return;

            context.Set<ApplicationRole>().Add(role);
            context.SaveChanges();
        });
    }

    public static void Seed(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((context, _) => { RolesSeed(context); });
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Database;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserSessionSql> UserSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.NationalId).IsUnique();
        });

        builder.Entity<UserSessionSql>(entity =>
        {
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()").ValueGeneratedOnAdd();
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAddOrUpdate();
        });
    }
}

using Identity.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public const string Schema = "identity";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);
        builder.Entity<ApplicationUser>().Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
        builder.Entity<ApplicationUser>().HasIndex(u => u.NormalizedEmail).IsUnique();
    }
}

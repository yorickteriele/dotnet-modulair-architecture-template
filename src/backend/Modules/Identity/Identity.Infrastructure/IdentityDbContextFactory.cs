using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection before using EF tooling.");
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connection, pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", IdentityDbContext.Schema));
        return new IdentityDbContext(options.Options);
    }
}

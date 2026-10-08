using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace __MODULE_NAME__.Infrastructure.Data;

public sealed class __MODULE_NAME__DbContextFactory : IDesignTimeDbContextFactory<__MODULE_NAME__DbContext>
{
    public __MODULE_NAME__DbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<__MODULE_NAME__DbContext>();
        var migrationsAssembly = typeof(__MODULE_NAME__DbContext).Assembly.GetName().Name
            ?? throw new InvalidOperationException("__MODULE_NAME__ migrations assembly name could not be determined.");

        optionsBuilder.UseNpgsql(GetConnectionString(), npgsql =>
        {
            npgsql.MigrationsAssembly(migrationsAssembly);
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, __MODULE_NAME__DbContext.Schema);
        });

        return new __MODULE_NAME__DbContext(optionsBuilder.Options);
    }

    private static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection before using EF tooling.");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Module.Abstractions;

public abstract class DbContextModuleBase<TDbContext> : ModuleBase
    where TDbContext : DbContext
{
    public sealed override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' was not found.");
        var migrationsAssembly = typeof(TDbContext).Assembly.GetName().Name
            ?? throw new InvalidOperationException($"{typeof(TDbContext).Name} migrations assembly name could not be determined.");

        services.AddDbContext<TDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(migrationsAssembly);
                npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schema);
            }));

        RegisterModuleServices(services, configuration);
    }

    public override async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var context = services.GetRequiredService<TDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }

    protected virtual string ConnectionStringName => "DefaultConnection";

    protected abstract string Schema { get; }

    protected abstract void RegisterModuleServices(IServiceCollection services, IConfiguration configuration);
}

using __MODULE_NAME__.Contracts.Schema;
using Microsoft.EntityFrameworkCore;

namespace __MODULE_NAME__.Infrastructure.Data;

public sealed class __MODULE_NAME__DbContext : DbContext
{
    public const string Schema = __MODULE_NAME__Schema.Name;

    public __MODULE_NAME__DbContext(DbContextOptions<__MODULE_NAME__DbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
    }
}

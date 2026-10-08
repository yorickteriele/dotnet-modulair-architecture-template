using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Module.Abstractions;

public abstract class ModuleBase : IModule
{
    public virtual string Name => GetModuleName();

    public virtual string ApiGroupName => Name.ToLowerInvariant();

    public abstract void RegisterServices(IServiceCollection services, IConfiguration configuration);

    public virtual void UseMiddleware(IApplicationBuilder app)
    {
    }

    public virtual void MapEndpoints(IEndpointRouteBuilder app)
    {
    }

    public virtual Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken) => Task.CompletedTask;

    private string GetModuleName()
    {
        const string suffix = "Module";
        var typeName = GetType().Name;

        return typeName.EndsWith(suffix, StringComparison.Ordinal)
            ? typeName[..^suffix.Length]
            : typeName;
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Module.Abstractions;

public interface IModule
{
    string Name { get; }

    // Group name for ApiExplorer/Swagger document segregation
    string ApiGroupName { get; }

    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    void UseMiddleware(IApplicationBuilder app);

    void MapEndpoints(IEndpointRouteBuilder app);

    Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken);
}

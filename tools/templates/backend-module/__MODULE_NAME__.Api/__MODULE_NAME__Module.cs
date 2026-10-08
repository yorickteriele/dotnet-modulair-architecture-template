using __MODULE_NAME__.Application.Services;
using __MODULE_NAME__.Contracts.Services;
using __MODULE_NAME__.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Module.Abstractions;

namespace __MODULE_NAME__.Api;

public sealed class __MODULE_NAME__Module : DbContextModuleBase<__MODULE_NAME__DbContext>
{
    protected override string Schema => __MODULE_NAME__DbContext.Schema;

    protected override void RegisterModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<I__MODULE_NAME__Service, __MODULE_NAME__Service>();
    }
}

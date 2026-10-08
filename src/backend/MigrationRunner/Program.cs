using Host.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
var modules = ModuleLoader.LoadModules();
var services = new ServiceCollection();
services.AddLogging();
foreach (var module in modules) module.RegisterServices(services, configuration);
await using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();
foreach (var module in modules.OrderBy(m => m.Name))
{
    Console.WriteLine($"Migrating {module.Name}...");
    await module.MigrateAsync(scope.ServiceProvider, CancellationToken.None);
}
Console.WriteLine("Database migrations completed.");

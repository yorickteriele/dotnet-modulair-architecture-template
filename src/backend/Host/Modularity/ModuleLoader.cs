using System.Reflection;
using System.Runtime.Loader;
using Module.Abstractions;

namespace Host.Modularity;

public static class ModuleLoader
{
    public static IReadOnlyCollection<IModule> LoadModules()
    {
        var loadedAssemblies = new List<Assembly>();
        var basePath = AppContext.BaseDirectory;
        var assemblyPaths = Directory.GetFiles(basePath, "*.Api.dll", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(basePath, "*.Infrastructure.dll", SearchOption.TopDirectoryOnly))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var assemblyPath in assemblyPaths)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            loadedAssemblies.Add(assembly);
        }

        var modules = loadedAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IModule).IsAssignableFrom(type))
            .Select(type => (IModule)Activator.CreateInstance(type)!)
            .ToList();

        return modules;
    }
}

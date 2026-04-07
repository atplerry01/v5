using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Platform.Host.Composition.Registry;

namespace Whyce.Platform.Host.Composition.Loader;

/// <summary>
/// Loader extension that walks the CompositionRegistry in deterministic
/// Order and registers each module. Behavior is identical to calling the
/// Add*Composition extensions in the locked sequence.
/// </summary>
public static class CompositionModuleLoader
{
    public static IServiceCollection LoadModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var ordered = CompositionRegistry.Modules
            .OrderBy(m => m.Order)
            .ToList();

        foreach (var module in ordered)
        {
            module.Register(services, configuration);
        }

        return services;
    }
}

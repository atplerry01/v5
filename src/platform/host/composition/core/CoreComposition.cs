using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Core;

/// <summary>
/// Core deterministic primitives (clock + id generator).
/// Extracted from Program.cs without behavior change.
/// </summary>
public static class CoreComposition
{
    public static IServiceCollection AddCoreComposition(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // IClock — deterministic time source (inject fixed clock for replay/test)
        services.AddSingleton<IClock, SystemClock>();

        // IIdGenerator — deterministic ID generation (SHA256-based)
        services.AddSingleton<IIdGenerator, DeterministicIdGenerator>();

        return services;
    }
}

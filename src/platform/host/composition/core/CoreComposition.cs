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

        // R2.A.1 — IRandomProvider: deterministic randomness (SHA256-based,
        // seed-driven, no hidden RNG state). Required by IRetryExecutor for
        // replay-safe jitter and by any future rebalance / probe / load-shed
        // selector under R-RETRY-DET-01.
        services.AddSingleton<Whycespace.Shared.Kernel.Domain.IRandomProvider,
            Whycespace.Runtime.Resilience.DeterministicRandomProvider>();

        // R2.A.1 — IRetryExecutor: canonical bounded retry with deterministic
        // backoff+jitter. Category-driven eligibility via RetryEligibility.
        services.AddSingleton<Whycespace.Shared.Contracts.Runtime.IRetryExecutor,
            Whycespace.Runtime.Resilience.DeterministicRetryExecutor>();

        return services;
    }
}

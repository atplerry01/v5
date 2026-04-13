using System.Collections.Generic;
using Whycespace.Platform.Host.Composition.Abstractions;
using Whycespace.Platform.Host.Composition.Core;
using Whycespace.Platform.Host.Composition.Infrastructure;
using Whycespace.Platform.Host.Composition.Observability;
using Whycespace.Platform.Host.Composition.Projections;
using Whycespace.Platform.Host.Composition.Runtime;

namespace Whycespace.Platform.Host.Composition.Registry;

/// <summary>
/// Deterministic, explicit composition module registry. No reflection, no
/// auto-discovery. Order here is the canonical execution order and must
/// match the locked sequence enforced by composition-loader.guard.md.
/// </summary>
public static class CompositionRegistry
{
    public static readonly IReadOnlyList<ICompositionModule> Modules = new List<ICompositionModule>
    {
        new CoreCompositionModule(),
        new RuntimeCompositionModule(),
        new InfrastructureCompositionModule(),
        new ProjectionCompositionModule(),
        new ObservabilityCompositionModule(),
    };
}

using System.Collections.Generic;
using Whyce.Platform.Host.Composition.Abstractions;
using Whyce.Platform.Host.Composition.Core;
using Whyce.Platform.Host.Composition.Infrastructure;
using Whyce.Platform.Host.Composition.Observability;
using Whyce.Platform.Host.Composition.Projections;
using Whyce.Platform.Host.Composition.Runtime;

namespace Whyce.Platform.Host.Composition.Registry;

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

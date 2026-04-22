using Whycespace.Platform.Host.Composition.Business;
using Whycespace.Platform.Host.Composition.Constitutional.Chain;
using Whycespace.Platform.Host.Composition.Constitutional.Policy;
using Whycespace.Platform.Host.Composition.Content;
using Whycespace.Platform.Host.Composition.Control;
using Whycespace.Platform.Host.Composition.Economic;
using Whycespace.Platform.Host.Composition.Integration.OutboundEffect;
using Whycespace.Platform.Host.Composition.Orchestration.Workflow;
using Whycespace.Platform.Host.Composition.Platform;
using Whycespace.Platform.Host.Composition.Structural;
using Whycespace.Platform.Host.Composition.Trust;

namespace Whycespace.Platform.Host.Composition;

/// <summary>
/// Static catalog of all domain bootstrap modules. The single seam where Program.cs
/// meets per-domain wiring — Program.cs imports only this catalog and never references
/// individual domain modules by name.
///
/// To onboard a new domain: add one entry to <see cref="All"/>.
/// </summary>
public static class BootstrapModuleCatalog
{
    public static IReadOnlyList<IDomainBootstrapModule> All { get; } =
    [
        new ConstitutionalPolicyBootstrap(),
        new ConstitutionalChainBootstrap(),
        new EconomicCompositionRoot(),
        new ContentSystemCompositionRoot(),
        new ControlSystemCompositionRoot(),
        new BusinessSystemCompositionRoot(),
        new StructuralSystemCompositionRoot(),
        new TrustSystemCompositionRoot(),
        new PlatformSystemCompositionRoot(),
        new WorkflowExecutionBootstrap(),
        new OutboundEffectsBootstrap()
    ];
}

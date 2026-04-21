using Whycespace.Platform.Host.Composition.Business;
using Whycespace.Platform.Host.Composition.Constitutional.Policy;
using Whycespace.Platform.Host.Composition.Content;
using Whycespace.Platform.Host.Composition.Economic;
using Whycespace.Platform.Host.Composition.Integration.OutboundEffect;
using Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban;
using Whycespace.Platform.Host.Composition.Operational.Sandbox.Todo;
using Whycespace.Platform.Host.Composition.Orchestration.Workflow;
using Whycespace.Platform.Host.Composition.Structural;

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
        new TodoCompositionRoot(),
        new KanbanCompositionRoot(),
        new EconomicCompositionRoot(),
        new ContentSystemCompositionRoot(),
        new BusinessSystemCompositionRoot(),
        new StructuralSystemCompositionRoot(),
        new WorkflowExecutionBootstrap(),
        new OutboundEffectsBootstrap()
    ];
}

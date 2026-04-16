using Whycespace.Platform.Host.Composition.Constitutional.Policy;
using Whycespace.Platform.Host.Composition.Content.Interaction.Messaging;
using Whycespace.Platform.Host.Composition.Content.Learning.Course;
using Whycespace.Platform.Host.Composition.Content.Media.Asset;
using Whycespace.Platform.Host.Composition.Economic;
using Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban;
using Whycespace.Platform.Host.Composition.Operational.Sandbox.Todo;
using Whycespace.Platform.Host.Composition.Orchestration.Workflow;

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
        new MediaAssetCompositionRoot(),
        new MessagingCompositionRoot(),
        new CourseCompositionRoot(),
        new EconomicCompositionRoot(),
        new WorkflowExecutionBootstrap()
    ];
}

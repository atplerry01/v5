using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Constitutional.Policy;

/// <summary>
/// Constitutional policy bootstrap module — registers the policy decision event
/// schema entries so EventFabric can persist, anchor, and publish the events
/// emitted by PolicyMiddleware.
///
/// Scope: schema only. No engines, projections, or workflows are owned by the
/// constitutional policy domain — those layers are populated by per-feature
/// bootstrap modules. Schema identity binding lives in the runtime-side
/// PolicyDecisionSchemaModule (Phase 1.5 §5.1.2 BPV-D01).
/// </summary>
public sealed class ConstitutionalPolicyBootstrap : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // No DI registrations — IPolicyDecisionEventFactory is registered by RuntimeComposition.
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterConstitutionalPolicyDecision(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        // No projections in this phase. Option-4 scope.
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        // No engines registered by this module — policy engine wiring lives in RuntimeComposition.
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No workflows.
    }
}

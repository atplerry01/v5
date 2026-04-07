using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Projection;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Platform.Host.Composition.Constitutional.Policy;

/// <summary>
/// Constitutional policy bootstrap module — registers the policy decision event
/// schema entries so EventFabric can persist, anchor, and publish the events
/// emitted by PolicyMiddleware.
///
/// Scope: schema only. No engines, projections, or workflows are owned by the
/// constitutional policy domain — those layers are populated by per-feature
/// bootstrap modules. Sits in <c>src/platform/host/composition/**</c> which is
/// the canonical exempt zone for domain-typed wiring (per rule 11.R-DOM-01).
/// </summary>
public sealed class ConstitutionalPolicyBootstrap : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // No DI registrations — IPolicyDecisionEventFactory is registered by RuntimeComposition.
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        schema.Register(
            "PolicyEvaluatedEvent",
            EventVersion.Default,
            typeof(Whycespace.Domain.ConstitutionalSystem.Policy.Decision.PolicyEvaluatedEvent),
            typeof(Whycespace.Domain.ConstitutionalSystem.Policy.Decision.PolicyEvaluatedEvent));

        schema.Register(
            "PolicyDeniedEvent",
            EventVersion.Default,
            typeof(Whycespace.Domain.ConstitutionalSystem.Policy.Decision.PolicyDeniedEvent),
            typeof(Whycespace.Domain.ConstitutionalSystem.Policy.Decision.PolicyDeniedEvent));
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

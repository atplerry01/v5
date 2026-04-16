namespace Whycespace.Shared.Contracts.Runtime;

// β #1 / D7: WorkflowId is now Guid (was string). The runtime dispatcher
// previously had to Guid.TryParse it on the way in; the conversion is now
// enforced at the contract boundary. IHasAggregateId lets
// SystemIntentDispatcher.ResolveAggregateId resolve the aggregate id without
// the property-name allowlist fallback.
public sealed record WorkflowResumeCommand(Guid WorkflowId) : IHasAggregateId
{
    public Guid AggregateId => WorkflowId;
}

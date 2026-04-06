using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Runtime orchestrator: subscribes to policy.violation events,
/// invokes enforcement engine, dispatches actions.
/// This is the ONLY path for enforcement — publisher does NOT trigger enforcement.
/// Idempotent via EnforcementIdGenerator (deterministic action IDs).
/// </summary>
public sealed class PolicyEnforcementOrchestrator : IEventConsumer
{
    private readonly IPolicyEnforcementPipeline _pipeline;

    public string EventType => "policy.violation";

    public PolicyEnforcementOrchestrator(IPolicyEnforcementPipeline pipeline)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    }

    public async Task HandleAsync(RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // Extract violation data from event payload
        var result = ExtractEvaluationResult(@event);
        if (result is null) return;

        var correlationId = @event.CorrelationId;

        await _pipeline.ExecuteAsync(result, correlationId, cancellationToken);
    }

    private static PolicyEvaluationResult? ExtractEvaluationResult(RuntimeEvent @event)
    {
        if (@event.Payload is null) return null;

        // The violation event carries enough data to reconstruct enforcement context
        return PolicyEvaluationResult.NonCompliant(
            "Violation detected via event consumer",
            source: PolicyExecutionSource.Domain);
    }
}

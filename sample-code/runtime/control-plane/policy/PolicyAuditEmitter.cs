using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Emits audit events for every policy evaluation decision.
/// Every command produces a PolicyEvaluatedEvent in the audit trail.
/// </summary>
public sealed class PolicyAuditEmitter
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IClock _clock;

    public PolicyAuditEmitter(IEventPublisher eventPublisher, IClock clock)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task EmitDecisionAsync(
        Guid commandId,
        string commandType,
        string correlationId,
        PolicyDecision decision,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = new RuntimeEvent
        {
            EventId = DeterministicIdHelper.FromSeed($"policy-audit:evaluated:{commandId}:{correlationId}"),
                AggregateId = Guid.Empty,
            EventType = "PolicyEvaluatedEvent",
            AggregateType = "PolicyAudit",
            CorrelationId = correlationId,
            CommandId = commandId,
            ExecutionId = commandType,
            Payload = new
            {
                commandId,
                commandType,
                decision = decision.Result.ToString(),
                decisionId = decision.DecisionId,
                policyIds = decision.PolicyIds,
                evaluationHash = decision.EvaluationHash,
                denialReason = decision.DenialReason,
                conditions = decision.Conditions,
                timestamp = decision.Timestamp
            },
            Timestamp = _clock.UtcNowOffset,
            Headers = new Dictionary<string, string>
            {
                ["x-audit-type"] = "policy-evaluation",
                ["x-decision-result"] = decision.Result.ToString()
            }
        };

        await _eventPublisher.PublishAsync(auditEvent, cancellationToken);
    }

    public async Task EmitViolationAsync(
        PolicyViolationEvent violation,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var violationEvent = new RuntimeEvent
        {
            EventId = DeterministicIdHelper.FromSeed($"policy-audit:violation:{violation.CommandId}:{correlationId}"),
                AggregateId = Guid.Empty,
            EventType = "PolicyViolationEvent",
            AggregateType = "PolicyAudit",
            CorrelationId = correlationId,
            CommandId = violation.CommandId,
            ExecutionId = violation.CommandType,
            Payload = new
            {
                violation.CommandId,
                violation.CommandType,
                violation.DecisionId,
                violation.Reason,
                violation.PolicyIds,
                violation.EvaluationHash,
                violation.Timestamp
            },
            Timestamp = _clock.UtcNowOffset,
            Headers = new Dictionary<string, string>
            {
                ["x-audit-type"] = "policy-violation",
                ["x-severity"] = "critical"
            }
        };

        await _eventPublisher.PublishAsync(violationEvent, cancellationToken);
    }
}

/// <summary>
/// Record representing a policy violation for audit emission.
/// </summary>
public sealed record PolicyViolationEvent
{
    public required Guid CommandId { get; init; }
    public required string CommandType { get; init; }
    public required Guid DecisionId { get; init; }
    public required string Reason { get; init; }
    public required IReadOnlyList<Guid> PolicyIds { get; init; }
    public required string EvaluationHash { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}

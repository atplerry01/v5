namespace Whycespace.Runtime.WhyceChain;

/// <summary>
/// Immutable record of a decision made during command execution.
/// Every policy evaluation, authorization check, and workflow routing decision
/// is captured as a DecisionEnvelope and emitted to WhyceChain for audit.
/// </summary>
public sealed record DecisionEnvelope
{
    public Guid DecisionId { get; init; }
    public required string ExecutionId { get; init; }
    public required Guid CommandId { get; init; }
    public required string CorrelationId { get; init; }
    public required string DecisionType { get; init; }
    public required string Outcome { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? Reason { get; init; }
    public string? PolicyId { get; init; }
    public IReadOnlyDictionary<string, string> Context { get; init; } = new Dictionary<string, string>();
}

public static class DecisionTypes
{
    public const string Authorization = "authorization";
    public const string PolicyEvaluation = "policy.evaluation";
    public const string WorkflowRouting = "workflow.routing";
    public const string EngineInvocation = "engine.invocation";
    public const string IdempotencyCheck = "idempotency.check";
    public const string ExecutionGuard = "execution.guard";
}

public static class DecisionOutcomes
{
    public const string Allowed = "allowed";
    public const string Denied = "denied";
    public const string Skipped = "skipped";
    public const string Compliant = "compliant";
    public const string NonCompliant = "non_compliant";
}

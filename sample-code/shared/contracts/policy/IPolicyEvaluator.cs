namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyEvaluator
{
    Task<PolicyEvaluationResult> EvaluateAsync(
        PolicyEvaluationInput input,
        CancellationToken cancellationToken = default);
}

public sealed record PolicyEvaluationInput(
    Guid? PolicyId,
    Guid ActorId,
    string Action,
    string Resource,
    string Environment,
    DateTimeOffset Timestamp)
{
    /// <summary>
    /// Domain-specific policy context (e.g., LedgerPolicyInput).
    /// When set, the policy evaluator MUST use this for domain-aware evaluation.
    /// When null, only generic policy evaluation is performed.
    /// </summary>
    public object? DomainContext { get; init; }
}

public sealed record PolicyEvaluationResult
{
    public required string DecisionType { get; init; }
    public required bool IsCompliant { get; init; }
    public string? Violation { get; init; }
    public IReadOnlyList<EvaluatedRule> EvaluatedRules { get; init; } = [];
    public IReadOnlyList<string> Violations { get; init; } = [];
    public PolicyEventData? EventPayload { get; init; }

    // WhyceChain audit fields
    public string? DecisionHash { get; init; }
    public IReadOnlyList<Guid> PolicyIds { get; init; } = [];
    public string? EvaluationTrace { get; init; }

    // Execution source tagging
    public PolicyExecutionSource Source { get; init; } = PolicyExecutionSource.Domain;

    // Chain anchoring directives — set by policy engine based on operation criticality
    public bool RequiresAnchoring { get; init; } = true;
    public string AnchoringMode { get; init; } = "ASYNC";

    public bool IsDenied => DecisionType == "DENY";
    public bool IsConditional => DecisionType == "CONDITIONAL";
    public bool IsStrictAnchoring => AnchoringMode == "STRICT";

    public static PolicyEvaluationResult Compliant(
        IReadOnlyList<EvaluatedRule>? rules = null,
        PolicyEventData? eventPayload = null,
        PolicyExecutionSource source = PolicyExecutionSource.Domain) => new()
    {
        DecisionType = "ALLOW",
        IsCompliant = true,
        EvaluatedRules = rules ?? [],
        EventPayload = eventPayload,
        Source = source
    };

    public static PolicyEvaluationResult NonCompliant(
        string violation,
        IReadOnlyList<EvaluatedRule>? rules = null,
        IReadOnlyList<string>? violations = null,
        PolicyEventData? eventPayload = null,
        PolicyExecutionSource source = PolicyExecutionSource.Domain) => new()
    {
        DecisionType = "DENY",
        IsCompliant = false,
        Violation = violation,
        EvaluatedRules = rules ?? [],
        Violations = violations ?? [violation],
        EventPayload = eventPayload,
        Source = source
    };

    public static PolicyEvaluationResult Conditional(
        IReadOnlyList<string> conditions,
        IReadOnlyList<EvaluatedRule>? rules = null,
        PolicyEventData? eventPayload = null,
        PolicyExecutionSource source = PolicyExecutionSource.Domain) => new()
    {
        DecisionType = "CONDITIONAL",
        IsCompliant = true,
        Violations = conditions,
        EvaluatedRules = rules ?? [],
        EventPayload = eventPayload,
        Source = source
    };
}

public sealed record EvaluatedRule(
    Guid RuleId,
    bool Passed,
    string? Reason = null);

public sealed record PolicyEventData(
    Guid PolicyId,
    string DecisionType,
    Guid ActorId,
    string Action,
    string Resource,
    string Environment,
    DateTimeOffset EvaluatedAt);

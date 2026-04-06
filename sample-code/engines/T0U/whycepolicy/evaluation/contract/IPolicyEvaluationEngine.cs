using Whycespace.Engines.T0U.WhycePolicy.Registry;

namespace Whycespace.Engines.T0U.WhycePolicy.Evaluation;

public interface IPolicyEvaluationEngine
{
    Task<PolicyEvaluationEngineResult> EvaluateAsync(
        PolicyContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Engine-local decision type — decoupled from domain PolicyDecisionType.
/// </summary>
public static class PolicyDecisionType
{
    public const string Allow = "ALLOW";
    public const string Deny = "DENY";
    public const string Conditional = "CONDITIONAL";
}

public sealed record PolicyEvaluationEngineResult(
    string DecisionType,
    IReadOnlyList<EvaluatedRuleResult> EvaluatedRules,
    IReadOnlyList<string> Violations,
    PolicyEvaluatedEventPayload? EventPayload = null)
{
    public bool IsAllowed => DecisionType == PolicyDecisionType.Allow;
    public bool IsDenied => DecisionType == PolicyDecisionType.Deny;
}

public sealed record EvaluatedRuleResult(
    Guid RuleId,
    bool Passed,
    string? Reason = null);

public sealed record PolicyEvaluatedEventPayload(
    Guid PolicyId,
    string DecisionType,
    Guid ActorId,
    string Action,
    string Resource,
    string Environment,
    DateTimeOffset EvaluatedAt);

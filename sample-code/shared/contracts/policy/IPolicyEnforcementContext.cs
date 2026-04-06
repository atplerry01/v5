namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyEnforcementContext
{
    string DecisionType { get; }
    IReadOnlyList<string> Violations { get; }
    IReadOnlyList<string> Conditions { get; }
    PolicyEventData? EventPayload { get; }
    bool IsAllowed { get; }
    bool IsDenied { get; }
    bool IsConditional { get; }
}

public sealed class PolicyEnforcementContext : IPolicyEnforcementContext
{
    public string DecisionType { get; }
    public IReadOnlyList<string> Violations { get; }
    public IReadOnlyList<string> Conditions { get; }
    public PolicyEventData? EventPayload { get; }

    public bool IsAllowed => DecisionType == "ALLOW";
    public bool IsDenied => DecisionType == "DENY";
    public bool IsConditional => DecisionType == "CONDITIONAL";

    private PolicyEnforcementContext(
        string decisionType,
        IReadOnlyList<string> violations,
        IReadOnlyList<string> conditions,
        PolicyEventData? eventPayload)
    {
        DecisionType = decisionType;
        Violations = violations;
        Conditions = conditions;
        EventPayload = eventPayload;
    }

    public static PolicyEnforcementContext FromResult(PolicyEvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var conditions = result.IsConditional ? result.Violations : [];

        return new PolicyEnforcementContext(
            result.DecisionType,
            result.Violations,
            conditions,
            result.EventPayload);
    }
}

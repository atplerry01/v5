namespace Whycespace.Projections.Chain;

/// <summary>
/// Indexed policy decision anchored to WhyceChain.
/// Projected from policy.decision.* chain events.
/// Key = "policy-decision:{correlationId}".
/// </summary>
public sealed record PolicyDecisionReadModel
{
    public required string CorrelationId { get; init; }
    public required string PolicyId { get; init; }
    public required string Decision { get; init; }
    public required string Subject { get; init; }
    public required string Resource { get; init; }
    public required string Action { get; init; }
    public required string EvaluationHash { get; init; }
    public required string BlockId { get; init; }
    public required string BlockHash { get; init; }
    public required DateTimeOffset AnchoredAt { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public long LastEventVersion { get; init; }

    // E5: Identity binding
    public string? SubjectId { get; init; }
    public string[]? Roles { get; init; }
    public double TrustScore { get; init; }
    public bool IsVerified { get; init; }

    public static string KeyFor(string correlationId) => $"policy-decision:{correlationId}";
    public static string KeyBySubject(string subjectId) => $"policy-decisions-by-subject:{subjectId}";
}

/// <summary>
/// Index of policy decisions by policyId for audit queries.
/// Key = "policy-decisions-by-policy:{policyId}".
/// </summary>
public sealed record PolicyDecisionIndexReadModel
{
    public required string PolicyId { get; init; }
    public List<string> CorrelationIds { get; init; } = [];
    public int DecisionCount { get; init; }
    public DateTimeOffset LastUpdated { get; init; }

    public static string KeyFor(string policyId) => $"policy-decisions-by-policy:{policyId}";
}

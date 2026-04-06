namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// An insight extracted from simulation/drift/anomaly analysis that supports a recommendation.
/// </summary>
public sealed class RecommendationInsight
{
    public Guid InsightId { get; }
    public InsightType Type { get; }
    public string Description { get; }
    public string Severity { get; }

    public RecommendationInsight(Guid insightId, InsightType type, string description, string severity)
    {
        if (insightId == Guid.Empty) throw new ArgumentException("InsightId must not be empty.", nameof(insightId));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (string.IsNullOrWhiteSpace(severity)) throw new ArgumentException("Severity is required.", nameof(severity));

        InsightId = insightId;
        Type = type;
        Description = description;
        Severity = severity;
    }
}

public enum InsightType
{
    Drift,
    Anomaly,
    Inefficiency,
    Conflict
}

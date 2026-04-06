namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Hierarchical explanation tree for audit-ready recommendation explainability.
/// Structure: insight → simulation_result → impact → risk → recommendation.
/// </summary>
public sealed record ExplanationTree
{
    public required string ReasoningSummary { get; init; }
    public required IReadOnlyList<ExplanationNode> Nodes { get; init; }
}

public sealed record ExplanationNode
{
    public required string Label { get; init; }
    public required string Category { get; init; }
    public required string Detail { get; init; }
    public IReadOnlyList<ExplanationNode> Children { get; init; } = [];
}

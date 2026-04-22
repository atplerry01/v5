namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDecision;

public sealed record PolicyDecisionReadModel
{
    public Guid DecisionId { get; init; }
    public string PolicyDefinitionId { get; init; } = string.Empty;
    public string SubjectId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Resource { get; init; } = string.Empty;
    public string Outcome { get; init; } = string.Empty;
    public DateTimeOffset DecidedAt { get; init; }
}

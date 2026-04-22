namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;

public sealed record PolicyEvaluationReadModel
{
    public Guid EvaluationId { get; init; }
    public string PolicyId { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public string? Outcome { get; init; }
    public string? DecisionHash { get; init; }
}

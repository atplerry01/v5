namespace Whycespace.Projections.IdentityIntelligence.ReadModels;

public sealed record RiskScoreReadModel
{
    public required string IdentityId { get; init; }
    public decimal Score { get; init; }
    public required string Severity { get; init; }
    public required IReadOnlyList<AnomalyFlagReadModel> Flags { get; init; }
    public DateTimeOffset ComputedAt { get; init; }
}

public sealed record AnomalyFlagReadModel
{
    public required string AnomalyType { get; init; }
    public required string Description { get; init; }
    public decimal Confidence { get; init; }
    public DateTimeOffset DetectedAt { get; init; }
}

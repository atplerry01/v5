namespace Whycespace.Engines.T3I.EconomicOptimization;

public sealed record OptimizationResultDto
{
    public required string OptimizationId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required string RecommendationType { get; init; }
    public required string RecommendationAction { get; init; }
    public required decimal ExpectedImpact { get; init; }
    public required decimal ConfidenceScore { get; init; }
    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string CorrelationId { get; init; }
    public required string SourceEventId { get; init; }
}

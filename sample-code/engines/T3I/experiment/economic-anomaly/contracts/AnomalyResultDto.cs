namespace Whycespace.Engines.T3I.EconomicAnomaly;

public sealed record AnomalyResultDto
{
    public required string AnomalyId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required decimal Deviation { get; init; }
    public required decimal DeviationPercentage { get; init; }
    public required string Severity { get; init; }
    public required decimal ConfidenceScore { get; init; }
    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string CorrelationId { get; init; }
    public required string SourceEventId { get; init; }
}

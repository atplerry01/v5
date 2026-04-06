namespace Whycespace.Projections.IdentityIntelligence.ReadModels;

public sealed record AnomalyReadModel
{
    public required string IdentityId { get; init; }
    public bool HasActiveAnomalies { get; init; }
    public required IReadOnlyList<AnomalyFlagReadModel> Anomalies { get; init; }
    public DateTimeOffset LastCheckedAt { get; init; }
}

namespace Whycespace.Projections.Global.IntelligenceAccuracy;

public sealed record IntelligenceAccuracyReadModel
{
    public required string Id { get; init; }
    public required string EngineId { get; init; }
    public long TotalPredictions { get; init; }
    public long CorrectPredictions { get; init; }
    public decimal AccuracyRate { get; init; }
    public long SuggestionsProposed { get; init; }
    public long SuggestionsApproved { get; init; }
    public long SuggestionsRejected { get; init; }
    public long SuggestionsActivated { get; init; }
    public decimal ApprovalRate { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}

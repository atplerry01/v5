namespace Whycespace.Projections.Chain;

/// <summary>
/// Chain anomaly detection derived from block events.
/// Detects hash mismatches, duplicate correlation attempts, and fork signals.
/// Key = "chain-anomaly:{anomalyId}" (one per detected anomaly).
/// </summary>
public sealed record ChainAnomalyReadModel
{
    public required string AnomalyId { get; init; }
    public required string AnomalyType { get; init; }
    public required string Description { get; init; }
    public required long AtSequenceNumber { get; init; }
    public required string BlockId { get; init; }
    public required DateTimeOffset DetectedAt { get; init; }
    public string? CorrelationId { get; init; }

    public static string KeyFor(string anomalyId) => $"chain-anomaly:{anomalyId}";
}

/// <summary>
/// Summary of all detected anomalies. Key = "chain-anomaly-summary" (singleton).
/// </summary>
public sealed record ChainAnomalySummaryReadModel
{
    public int TotalAnomalies { get; init; }
    public int HashMismatches { get; init; }
    public int DuplicateCorrelations { get; init; }
    public int ForkAttempts { get; init; }
    public DateTimeOffset LastAnomalyAt { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public long LastEventVersion { get; init; }

    public const string Key = "chain-anomaly-summary";
}

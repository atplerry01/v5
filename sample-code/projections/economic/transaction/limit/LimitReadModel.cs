namespace Whycespace.Projections.Economic;

/// <summary>
/// Limit usage tracking per identity.
/// Daily and monthly usage derived from transaction.* and limit.exceeded events.
/// Buckets derived from event.Timestamp (source of truth):
///   DailyBucket  = YYYY-MM-DD
///   MonthlyBucket = YYYY-MM
/// </summary>
public sealed record LimitUsageReadModel
{
    public required string IdentityId { get; init; }
    public decimal DailyUsage { get; init; }
    public decimal MonthlyUsage { get; init; }
    public int DailyTransactionCount { get; init; }
    public int MonthlyTransactionCount { get; init; }
    public string DailyBucket { get; init; } = "";
    public string MonthlyBucket { get; init; } = "";
    public int LimitExceededCount { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}

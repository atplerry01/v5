namespace Whycespace.Shared.Contracts.Economic.Revenue.Payout;

public sealed record PayoutReadModel
{
    public Guid PayoutId { get; init; }
    public Guid DistributionId { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    // Phase 7 B2 — compensation projection fields. Populated when the
    // payout transitions through CompensationRequested / Compensated;
    // null on Standard payouts. JSONB round-trip preserves legacy rows:
    // absent keys deserialize to defaults without schema migration.
    public string CompensationReason { get; init; } = string.Empty;
    public string CompensatingJournalId { get; init; } = string.Empty;
    public DateTimeOffset? CompensationRequestedAt { get; init; }
    public DateTimeOffset? CompensatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}

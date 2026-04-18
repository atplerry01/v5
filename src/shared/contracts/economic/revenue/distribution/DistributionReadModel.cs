namespace Whycespace.Shared.Contracts.Economic.Revenue.Distribution;

public sealed record DistributionReadModel
{
    public Guid DistributionId { get; init; }
    public string SpvId { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;

    // Phase 7 B1 — compensation projection fields. Populated when the
    // distribution transitions through CompensationRequested / Compensated.
    // OriginalPayoutId is the correlation anchor to the sibling payout
    // whose reversal drove this compensation (enforced by the aggregate's
    // CompensationCorrelationRequired invariant).
    public string OriginalPayoutId { get; init; } = string.Empty;
    public string CompensationReason { get; init; } = string.Empty;
    public string CompensatingJournalId { get; init; } = string.Empty;
    public DateTimeOffset? CompensationRequestedAt { get; init; }
    public DateTimeOffset? CompensatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}

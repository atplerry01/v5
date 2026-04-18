using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

/// <summary>
/// V1 wire shape: <c>(PayoutId, DistributionId)</c> — preserved as the
/// positional constructor so historic event-store rows replay cleanly.
/// V2 fields (<c>IdempotencyKey</c>, <c>ExecutedAt</c>) are init-only properties
/// with safe defaults so missing JSON keys deserialize to empty / default
/// without throwing. New emissions populate both via object initializer.
/// Phase 3.5 T3.5.1 — additive evolution per Option B.
/// </summary>
public sealed record PayoutExecutedEvent(
    string PayoutId,
    string DistributionId) : DomainEvent
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public Timestamp ExecutedAt { get; init; }
}

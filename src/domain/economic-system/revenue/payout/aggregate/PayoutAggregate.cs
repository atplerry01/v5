using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

/// <summary>
/// Lifecycle aggregate for a payout instance. Pure domain state.
///
/// State machine (Phase 7 T7.2):
///   Requested -> Executed  -> CompensationRequested -> Compensated
///   Requested -> Failed    -> CompensationRequested -> Compensated
/// Compensated is terminal and irreversible.
///
/// PayoutId is derived upstream from (DistributionId, SpvId) via IIdGenerator
/// so retries converge on the same instance and replay is idempotent (T3.4).
///
/// T7.5 — Idempotent retry: a new payout for the same (DistributionId, SpvId)
/// is only permitted when the prior payout aggregate is in Compensated or
/// Failed state. That check lives in the engine request step (which loads
/// the prior aggregate), not in this factory — the aggregate itself is a
/// single payout instance with no knowledge of other attempts.
///
/// Replay safety (Phase 3.5 T3.5.1, Option B — additive evolution):
/// - V1 streams contain only PayoutExecutedEvent (no Requested precursor),
///   with the V1 shape <c>(PayoutId, DistributionId)</c>. The Apply switch
///   below tolerates this by hydrating PayoutId / DistributionId from the
///   Executed event itself when no Requested has been applied yet, and by
///   accepting an empty IdempotencyKey (legacy rows had no key).
/// - V2 streams begin with PayoutRequestedEvent, which fully hydrates state
///   before MarkExecuted runs.
/// </summary>
public sealed class PayoutAggregate : AggregateRoot
{
    public PayoutId PayoutId { get; private set; }
    public DistributionId DistributionId { get; private set; }
    public PayoutIdempotencyKey IdempotencyKey { get; private set; }
    public PayoutStatus Status { get; private set; }

    private PayoutAggregate() { }

    public static PayoutAggregate Request(
        PayoutId payoutId,
        DistributionId distributionId,
        PayoutIdempotencyKey idempotencyKey,
        IReadOnlyList<ParticipantShare> shares,
        Timestamp requestedAt)
    {
        if (shares is null || shares.Count == 0)
            throw PayoutErrors.SharesRequired();

        var total = 0m;
        foreach (var s in shares) total += s.Amount;
        if (total <= 0m)
            throw PayoutErrors.TotalMustBePositive();

        var agg = new PayoutAggregate();
        agg.RaiseDomainEvent(new PayoutRequestedEvent(
            payoutId.Value.ToString(),
            distributionId.Value.ToString(),
            idempotencyKey.Value,
            requestedAt));
        return agg;
    }

    public void MarkExecuted(Timestamp executedAt)
    {
        if (Status == PayoutStatus.Executed) return;
        if (Status != PayoutStatus.Requested)
            throw PayoutErrors.InvalidTransition(Status, PayoutStatus.Executed);

        RaiseDomainEvent(new PayoutExecutedEvent(
            PayoutId.Value.ToString(),
            DistributionId.Value.ToString())
        {
            IdempotencyKey = IdempotencyKey.Value,
            ExecutedAt = executedAt
        });
    }

    public void MarkFailed(string reason, Timestamp failedAt)
    {
        if (Status == PayoutStatus.Executed || Status == PayoutStatus.Failed)
            throw PayoutErrors.AlreadyTerminal();

        RaiseDomainEvent(new PayoutFailedEvent(
            PayoutId.Value.ToString(),
            DistributionId.Value.ToString(),
            reason ?? string.Empty,
            failedAt));
    }

    public void RequestCompensation(string reason, Timestamp requestedAt)
    {
        if (Status == PayoutStatus.Compensated || Status == PayoutStatus.CompensationRequested)
            throw PayoutErrors.AlreadyCompensated();

        if (Status != PayoutStatus.Executed && Status != PayoutStatus.Failed)
            throw PayoutErrors.CompensationNotAllowed(Status);

        RaiseDomainEvent(new PayoutCompensationRequestedEvent(
            PayoutId.Value.ToString(),
            DistributionId.Value.ToString(),
            IdempotencyKey.Value,
            reason ?? string.Empty,
            requestedAt));
    }

    public void MarkCompensated(string compensatingJournalId, Timestamp compensatedAt)
    {
        if (string.IsNullOrWhiteSpace(compensatingJournalId))
            throw PayoutErrors.CompensatingJournalIdRequired();

        if (Status == PayoutStatus.Compensated)
            throw PayoutErrors.AlreadyCompensated();
        if (Status != PayoutStatus.CompensationRequested)
            throw PayoutErrors.CompensationNotRequested();

        RaiseDomainEvent(new PayoutCompensatedEvent(
            PayoutId.Value.ToString(),
            DistributionId.Value.ToString(),
            IdempotencyKey.Value,
            compensatingJournalId,
            compensatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PayoutRequestedEvent e:
                PayoutId = PayoutId.From(Guid.Parse(e.PayoutId));
                DistributionId = DistributionId.From(Guid.Parse(e.DistributionId));
                IdempotencyKey = PayoutIdempotencyKey.From(e.IdempotencyKey);
                Status = PayoutStatus.Requested;
                break;
            case PayoutExecutedEvent e:
                if (PayoutId.Value == Guid.Empty)
                    PayoutId = PayoutId.From(Guid.Parse(e.PayoutId));
                if (DistributionId.Value == Guid.Empty)
                    DistributionId = DistributionId.From(Guid.Parse(e.DistributionId));
                if (string.IsNullOrEmpty(IdempotencyKey.Value))
                    IdempotencyKey = PayoutIdempotencyKey.From(
                        string.IsNullOrEmpty(e.IdempotencyKey) ? "legacy" : e.IdempotencyKey);
                Status = PayoutStatus.Executed;
                break;
            case PayoutFailedEvent:
                Status = PayoutStatus.Failed;
                break;
            case PayoutCompensationRequestedEvent:
                Status = PayoutStatus.CompensationRequested;
                break;
            case PayoutCompensatedEvent:
                Status = PayoutStatus.Compensated;
                break;
        }
    }
}

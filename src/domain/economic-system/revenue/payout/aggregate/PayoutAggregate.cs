using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

/// <summary>
/// Records the intent that a payout has been executed. Pure domain state;
/// emits a single intent event. No vault references, no cross-aggregate
/// calls — full SPV-debit / participant-credit conservation is the
/// responsibility of the Phase 2D orchestration layer.
/// </summary>
public sealed class PayoutAggregate : AggregateRoot
{
    public PayoutId PayoutId { get; private set; }
    public DistributionId DistributionId { get; private set; }
    public PayoutStatus Status { get; private set; }

    private PayoutAggregate() { }

    public static PayoutAggregate ExecutePayout(
        string payoutId,
        string distributionId,
        IReadOnlyList<ParticipantShare> shares)
    {
        if (shares is null || shares.Count == 0)
            throw new ArgumentException("Shares cannot be empty");

        var total = shares.Sum(s => s.Amount);

        if (total <= 0)
            throw new InvalidOperationException("Total payout must be > 0");

        var agg = new PayoutAggregate();

        agg.RaiseDomainEvent(new PayoutExecutedEvent(
            payoutId,
            distributionId
        ));

        return agg;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PayoutExecutedEvent e:
                PayoutId = PayoutId.From(Guid.Parse(e.PayoutId));
                DistributionId = DistributionId.From(Guid.Parse(e.DistributionId));
                Status = PayoutStatus.Completed;
                break;
        }
    }
}

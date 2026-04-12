using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionAggregate : AggregateRoot
{
    private readonly List<Allocation> _allocations = new();

    public DistributionId DistributionId { get; private set; }
    public Guid RevenueId { get; private set; }
    public Amount TotalAmount { get; private set; }
    public Currency Currency { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public IReadOnlyList<Allocation> Allocations => _allocations.AsReadOnly();

    private DistributionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DistributionAggregate Distribute(
        DistributionId distributionId,
        Guid revenueId,
        Amount totalAmount,
        Currency currency,
        Timestamp createdAt)
    {
        if (totalAmount.Value <= 0m) throw DistributionErrors.InvalidAmount();
        if (revenueId == Guid.Empty) throw DistributionErrors.MissingRevenueReference();

        var aggregate = new DistributionAggregate();
        aggregate.RaiseDomainEvent(new DistributionCreatedEvent(
            distributionId, revenueId, totalAmount, currency, createdAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void AssignAllocation(Guid recipientId, Amount allocationAmount, decimal sharePercentage)
    {
        if (recipientId == Guid.Empty) throw DistributionErrors.InvalidRecipient();
        if (sharePercentage <= 0m || sharePercentage > 100m) throw DistributionErrors.InvalidSharePercentage();

        RaiseDomainEvent(new AllocationAssignedEvent(
            DistributionId, recipientId, allocationAmount, sharePercentage));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DistributionCreatedEvent e:
                DistributionId = e.DistributionId;
                RevenueId = e.RevenueId;
                TotalAmount = e.TotalAmount;
                Currency = e.Currency;
                CreatedAt = e.CreatedAt;
                break;

            case AllocationAssignedEvent e:
                _allocations.Add(Allocation.Create(e.RecipientId, e.AllocationAmount, e.SharePercentage));
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (TotalAmount.Value < 0m) throw DistributionErrors.NegativeDistributionAmount();

        if (_allocations.Count > 0)
        {
            var allocationsSum = 0m;
            foreach (var allocation in _allocations)
                allocationsSum += allocation.Amount.Value;

            if (allocationsSum > TotalAmount.Value)
                throw DistributionErrors.AllocationsSumMismatch(new Amount(allocationsSum), TotalAmount);
        }
    }
}

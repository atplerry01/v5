using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

/// <summary>
/// Revenue recorded against an SPV. Per doctrine, revenue originates from
/// an SPV and is recorded before being normalized into the SPV's vault
/// (Slice1 via VaultAccountAggregate.ApplyRevenue). This aggregate records
/// the fact only — no vault logic here.
/// </summary>
public sealed class RevenueAggregate : AggregateRoot
{
    public RevenueId RevenueId { get; private set; }
    public string SpvId { get; private set; } = string.Empty;
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public string SourceRef { get; private set; } = string.Empty;
    public RevenueStatus Status { get; private set; }

    private RevenueAggregate() { }

    public static RevenueAggregate RecordRevenue(
        RevenueId revenueId,
        string spvId,
        decimal amount,
        string currency,
        string sourceRef)
    {
        if (amount <= 0m)
            throw new ArgumentException("Revenue amount must be greater than zero.", nameof(amount));

        // Per prompt: SpvId existence is assumed valid — no structural lookup here.

        var aggregate = new RevenueAggregate();
        aggregate.RaiseDomainEvent(new RevenueRecordedEvent(
            revenueId.Value.ToString(),
            spvId,
            amount,
            currency,
            sourceRef));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RevenueRecordedEvent e:
                RevenueId = RevenueId.From(Guid.Parse(e.RevenueId));
                SpvId = e.SpvId;
                Amount = new Amount(e.Amount);
                Currency = new Currency(e.Currency);
                SourceRef = e.SourceRef;
                Status = RevenueStatus.Recorded;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Status == RevenueStatus.Recorded && Amount.Value <= 0m)
            throw new DomainInvariantViolationException(
                "Invariant violated: recorded revenue amount must be greater than zero.");
    }
}

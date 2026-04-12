using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed class RevenueAggregate : AggregateRoot
{
    public RevenueId RevenueId { get; private set; }
    public Guid ContractId { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public RevenueStatus Status { get; private set; }
    public Timestamp RecognizedAt { get; private set; }

    private RevenueAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RevenueAggregate Recognize(
        RevenueId revenueId,
        Guid contractId,
        Amount amount,
        Currency currency,
        Timestamp recognizedAt)
    {
        if (amount.Value <= 0m) throw RevenueErrors.InvalidAmount();
        if (contractId == Guid.Empty) throw RevenueErrors.MissingContractReference();

        var aggregate = new RevenueAggregate();
        aggregate.RaiseDomainEvent(new RevenueRecognizedEvent(
            revenueId, contractId, amount, currency, recognizedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void MarkDistributed(Timestamp distributedAt)
    {
        if (Status == RevenueStatus.Distributed) throw RevenueErrors.RevenueAlreadyDistributed();
        if (Status != RevenueStatus.Recognized) throw RevenueErrors.RevenueNotRecognized();

        RaiseDomainEvent(new RevenueDistributedEvent(RevenueId, distributedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RevenueRecognizedEvent e:
                RevenueId = e.RevenueId;
                ContractId = e.ContractId;
                Amount = e.Amount;
                Currency = e.Currency;
                Status = RevenueStatus.Recognized;
                RecognizedAt = e.RecognizedAt;
                break;

            case RevenueDistributedEvent:
                Status = RevenueStatus.Distributed;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Amount.Value < 0m) throw RevenueErrors.NegativeRevenue();
    }
}

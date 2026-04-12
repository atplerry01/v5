using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public sealed class PricingAggregate : AggregateRoot
{
    public PricingId PricingId { get; private set; }
    public Guid ContractId { get; private set; }
    public PricingModel Model { get; private set; }
    public Amount Price { get; private set; }
    public Currency Currency { get; private set; }
    public Timestamp DefinedAt { get; private set; }

    private PricingAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static PricingAggregate DefinePrice(
        PricingId pricingId,
        Guid contractId,
        PricingModel model,
        Amount price,
        Currency currency,
        Timestamp definedAt)
    {
        if (price.Value <= 0m) throw PricingErrors.InvalidPrice();
        if (contractId == Guid.Empty) throw PricingErrors.MissingContractReference();

        var aggregate = new PricingAggregate();
        aggregate.RaiseDomainEvent(new PriceDefinedEvent(
            pricingId, contractId, model, price, currency, definedAt));
        return aggregate;
    }

    // ── Adjust ───────────────────────────────────────────────────

    public void AdjustPrice(Amount newPrice, string reason, Timestamp adjustedAt)
    {
        var specification = new CanAdjustPriceSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw PricingErrors.InvalidPrice();

        if (newPrice.Value <= 0m) throw PricingErrors.InvalidPrice();
        if (string.IsNullOrWhiteSpace(reason)) throw PricingErrors.MissingAdjustmentReason();

        RaiseDomainEvent(new PriceAdjustedEvent(
            PricingId, Price, newPrice, reason, adjustedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PriceDefinedEvent e:
                PricingId = e.PricingId;
                ContractId = e.ContractId;
                Model = e.Model;
                Price = e.Price;
                Currency = e.Currency;
                DefinedAt = e.DefinedAt;
                break;

            case PriceAdjustedEvent e:
                Price = e.NewPrice;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Price.Value < 0m) throw PricingErrors.NegativePrice();
        if (ContractId == Guid.Empty) throw PricingErrors.ContractReferenceMustExist();
    }
}

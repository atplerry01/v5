using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Pricing;

public sealed class PricingPlanAggregate : AggregateRoot
{
    private static readonly PricingSpecification Spec = new();

    public PricingPlanId PlanId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public MoneyAmount? Price { get; private set; }
    public PricingPlanStatus Status { get; private set; }
    public Timestamp DefinedAt { get; private set; }

    private PricingPlanAggregate() { }

    public static PricingPlanAggregate Define(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        PricingPlanId id, string name, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(name)) throw PricingErrors.InvalidName();
        var agg = new PricingPlanAggregate();
        agg.RaiseDomainEvent(new PricingPlanDefinedEvent(eventId, aggregateId, correlationId, causationId, id, name.Trim(), at));
        return agg;
    }

    public void AdjustPrice(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, MoneyAmount price, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        RaiseDomainEvent(new PricingPriceAdjustedEvent(eventId, aggregateId, correlationId, causationId, PlanId, price.Amount, price.CurrencyCode, at));
    }

    public void Publish(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsurePublishable(Status, Price is not null);
        RaiseDomainEvent(new PricingPlanPublishedEvent(eventId, aggregateId, correlationId, causationId, PlanId, at));
    }

    public void Deprecate(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == PricingPlanStatus.Deprecated) throw PricingErrors.AlreadyDeprecated();
        RaiseDomainEvent(new PricingPlanDeprecatedEvent(eventId, aggregateId, correlationId, causationId, PlanId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PricingPlanDefinedEvent e:
                PlanId = e.PlanId;
                Name = e.Name;
                Status = PricingPlanStatus.Draft;
                DefinedAt = e.DefinedAt;
                break;
            case PricingPriceAdjustedEvent e:
                Price = MoneyAmount.Create(e.Amount, e.CurrencyCode);
                break;
            case PricingPlanPublishedEvent: Status = PricingPlanStatus.Published; break;
            case PricingPlanDeprecatedEvent: Status = PricingPlanStatus.Deprecated; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(Name))
            throw PricingErrors.NameMissing();
    }
}

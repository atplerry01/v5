using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Subscription;

public sealed class SubscriptionAggregate : AggregateRoot
{
    private static readonly SubscriptionSpecification Spec = new();

    public SubscriptionId SubscriptionId { get; private set; }
    public string SubscriberRef { get; private set; } = string.Empty;
    public string PlanRef { get; private set; } = string.Empty;
    public BillingCycle Cycle { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public Timestamp PeriodStart { get; private set; }
    public Timestamp PeriodEnd { get; private set; }

    private SubscriptionAggregate() { }

    public static SubscriptionAggregate Create(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        SubscriptionId id, string subscriberRef, string planRef, BillingCycle cycle, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(subscriberRef)) throw SubscriptionErrors.InvalidSubscriber();
        if (string.IsNullOrWhiteSpace(planRef)) throw SubscriptionErrors.InvalidPlan();
        var agg = new SubscriptionAggregate();
        agg.RaiseDomainEvent(new SubscriptionCreatedEvent(eventId, aggregateId, correlationId, causationId, id, subscriberRef, planRef, cycle, at));
        return agg;
    }

    public void Activate(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp periodStart, Timestamp periodEnd, Timestamp at)
    {
        Spec.EnsureActivatable(Status);
        Spec.EnsurePeriod(periodStart, periodEnd);
        RaiseDomainEvent(new SubscriptionActivatedEvent(eventId, aggregateId, correlationId, causationId, SubscriptionId, periodStart, periodEnd, at));
    }

    public void Renew(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp periodStart, Timestamp periodEnd, Timestamp at)
    {
        Spec.EnsureRenewable(Status);
        Spec.EnsurePeriod(periodStart, periodEnd);
        RaiseDomainEvent(new SubscriptionRenewedEvent(eventId, aggregateId, correlationId, causationId, SubscriptionId, periodStart, periodEnd, at));
    }

    public void Cancel(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string reason, Timestamp at)
    {
        if (Status == SubscriptionStatus.Cancelled) throw SubscriptionErrors.AlreadyCancelled();
        RaiseDomainEvent(new SubscriptionCancelledEvent(eventId, aggregateId, correlationId, causationId, SubscriptionId, reason ?? string.Empty, at));
    }

    public void Expire(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == SubscriptionStatus.Expired) throw SubscriptionErrors.AlreadyExpired();
        RaiseDomainEvent(new SubscriptionExpiredEvent(eventId, aggregateId, correlationId, causationId, SubscriptionId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SubscriptionCreatedEvent e:
                SubscriptionId = e.SubscriptionId;
                SubscriberRef = e.SubscriberRef;
                PlanRef = e.PlanRef;
                Cycle = e.Cycle;
                Status = SubscriptionStatus.Created;
                break;
            case SubscriptionActivatedEvent e:
                Status = SubscriptionStatus.Active;
                PeriodStart = e.PeriodStart;
                PeriodEnd = e.PeriodEnd;
                break;
            case SubscriptionRenewedEvent e:
                PeriodStart = e.PeriodStart;
                PeriodEnd = e.PeriodEnd;
                break;
            case SubscriptionCancelledEvent: Status = SubscriptionStatus.Cancelled; break;
            case SubscriptionExpiredEvent: Status = SubscriptionStatus.Expired; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(SubscriberRef))
            throw SubscriptionErrors.SubscriberMissing();
    }
}

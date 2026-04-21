using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public sealed class RenewalAggregate : AggregateRoot
{
    public RenewalId Id { get; private set; }
    public RenewalSourceId SourceId { get; private set; }
    public RenewalStatus Status { get; private set; }

    public static RenewalAggregate Create(RenewalId id, RenewalSourceId sourceId)
    {
        var aggregate = new RenewalAggregate();
        if (aggregate.Version >= 0)
            throw RenewalErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new RenewalCreatedEvent(id, sourceId));
        return aggregate;
    }

    public void Renew()
    {
        var specification = new CanRenewSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RenewalErrors.InvalidStateTransition(Status, nameof(Renew));

        RaiseDomainEvent(new RenewalRenewedEvent(Id));
    }

    public void Expire()
    {
        var specification = new CanExpireRenewalSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RenewalErrors.InvalidStateTransition(Status, nameof(Expire));

        RaiseDomainEvent(new RenewalExpiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RenewalCreatedEvent e:
                Id = e.RenewalId;
                SourceId = e.SourceId;
                Status = RenewalStatus.Pending;
                break;
            case RenewalRenewedEvent:
                Status = RenewalStatus.Renewed;
                break;
            case RenewalExpiredEvent:
                Status = RenewalStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw RenewalErrors.MissingId();

        if (SourceId == default)
            throw RenewalErrors.MissingSourceId();

        if (!Enum.IsDefined(Status))
            throw RenewalErrors.InvalidStateTransition(Status, "validate");
    }
}

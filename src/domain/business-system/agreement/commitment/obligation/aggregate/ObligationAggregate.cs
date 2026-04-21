using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;

public sealed class ObligationAggregate : AggregateRoot
{
    public ObligationId Id { get; private set; }
    public ObligationStatus Status { get; private set; }

    public static ObligationAggregate Create(ObligationId id)
    {
        var aggregate = new ObligationAggregate();
        if (aggregate.Version >= 0)
            throw ObligationErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ObligationCreatedEvent(id));
        return aggregate;
    }

    public void Fulfill()
    {
        var specification = new CanFulfillSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ObligationErrors.InvalidStateTransition(Status, nameof(Fulfill));

        RaiseDomainEvent(new ObligationFulfilledEvent(Id));
    }

    public void Breach()
    {
        var specification = new CanBreachSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ObligationErrors.InvalidStateTransition(Status, nameof(Breach));

        RaiseDomainEvent(new ObligationBreachedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ObligationCreatedEvent e:
                Id = e.ObligationId;
                Status = ObligationStatus.Pending;
                break;
            case ObligationFulfilledEvent:
                Status = ObligationStatus.Fulfilled;
                break;
            case ObligationBreachedEvent:
                Status = ObligationStatus.Breached;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ObligationErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ObligationErrors.InvalidStateTransition(Status, "validate");
    }
}

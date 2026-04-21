using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;

public sealed class ValidityAggregate : AggregateRoot
{
    public ValidityId Id { get; private set; }
    public ValidityStatus Status { get; private set; }

    public static ValidityAggregate Create(ValidityId id)
    {
        var aggregate = new ValidityAggregate();
        if (aggregate.Version >= 0)
            throw ValidityErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ValidityCreatedEvent(id));
        return aggregate;
    }

    public void Invalidate()
    {
        var specification = new CanInvalidateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ValidityErrors.InvalidStateTransition(Status, nameof(Invalidate));

        RaiseDomainEvent(new ValidityInvalidatedEvent(Id));
    }

    public void Expire()
    {
        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ValidityErrors.InvalidStateTransition(Status, nameof(Expire));

        RaiseDomainEvent(new ValidityExpiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ValidityCreatedEvent e:
                Id = e.ValidityId;
                Status = ValidityStatus.Valid;
                break;
            case ValidityInvalidatedEvent:
                Status = ValidityStatus.Invalid;
                break;
            case ValidityExpiredEvent:
                Status = ValidityStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ValidityErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ValidityErrors.InvalidStateTransition(Status, "validate");
    }
}

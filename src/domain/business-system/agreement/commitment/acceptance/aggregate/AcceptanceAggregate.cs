using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public sealed class AcceptanceAggregate : AggregateRoot
{
    public AcceptanceId Id { get; private set; }
    public AcceptanceStatus Status { get; private set; }

    public static AcceptanceAggregate Create(AcceptanceId id)
    {
        var aggregate = new AcceptanceAggregate();
        if (aggregate.Version >= 0)
            throw AcceptanceErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AcceptanceCreatedEvent(id));
        return aggregate;
    }

    public void Accept()
    {
        var specification = new CanAcceptSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AcceptanceErrors.InvalidStateTransition(Status, nameof(Accept));

        RaiseDomainEvent(new AcceptanceAcceptedEvent(Id));
    }

    public void Reject()
    {
        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AcceptanceErrors.InvalidStateTransition(Status, nameof(Reject));

        RaiseDomainEvent(new AcceptanceRejectedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AcceptanceCreatedEvent e:
                Id = e.AcceptanceId;
                Status = AcceptanceStatus.Pending;
                break;
            case AcceptanceAcceptedEvent:
                Status = AcceptanceStatus.Accepted;
                break;
            case AcceptanceRejectedEvent:
                Status = AcceptanceStatus.Rejected;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw AcceptanceErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw AcceptanceErrors.InvalidStateTransition(Status, "validate");
    }
}

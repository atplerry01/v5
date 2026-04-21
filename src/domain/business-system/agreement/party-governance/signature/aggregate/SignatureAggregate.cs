using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;

public sealed class SignatureAggregate : AggregateRoot
{
    public SignatureId Id { get; private set; }
    public SignatureStatus Status { get; private set; }

    public static SignatureAggregate Create(SignatureId id)
    {
        var aggregate = new SignatureAggregate();
        if (aggregate.Version >= 0)
            throw SignatureErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new SignatureCreatedEvent(id));
        return aggregate;
    }

    public void Sign()
    {
        var specification = new CanSignSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SignatureErrors.InvalidStateTransition(Status, nameof(Sign));

        RaiseDomainEvent(new SignatureSignedEvent(Id));
    }

    public void Revoke()
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SignatureErrors.InvalidStateTransition(Status, nameof(Revoke));

        RaiseDomainEvent(new SignatureRevokedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SignatureCreatedEvent e:
                Id = e.SignatureId;
                Status = SignatureStatus.Pending;
                break;
            case SignatureSignedEvent:
                Status = SignatureStatus.Signed;
                break;
            case SignatureRevokedEvent:
                Status = SignatureStatus.Revoked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw SignatureErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SignatureErrors.InvalidStateTransition(Status, "validate");
    }
}

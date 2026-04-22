using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed class CredentialAggregate : AggregateRoot
{
    public CredentialId Id { get; private set; }
    public CredentialDescriptor Descriptor { get; private set; }
    public CredentialStatus Status { get; private set; }

    private CredentialAggregate() { }

    public static CredentialAggregate Issue(CredentialId id, CredentialDescriptor descriptor)
    {
        if (id == default)
            throw CredentialErrors.MissingId();
        if (descriptor == default)
            throw CredentialErrors.MissingDescriptor();

        var aggregate = new CredentialAggregate();
        aggregate.RaiseDomainEvent(new CredentialIssuedEvent(id, descriptor));
        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CredentialErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new CredentialActivatedEvent(Id));
    }

    public void Revoke()
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CredentialErrors.InvalidStateTransition(Status, nameof(Revoke));

        RaiseDomainEvent(new CredentialRevokedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CredentialIssuedEvent e:
                Id = e.CredentialId;
                Descriptor = e.Descriptor;
                Status = CredentialStatus.Issued;
                break;
            case CredentialActivatedEvent:
                Status = CredentialStatus.Active;
                break;
            case CredentialRevokedEvent:
                Status = CredentialStatus.Revoked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw CredentialErrors.MissingId();

        if (Descriptor == default)
            throw CredentialErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw new InvalidOperationException("CredentialStatus is not a defined enum value.");
    }
}

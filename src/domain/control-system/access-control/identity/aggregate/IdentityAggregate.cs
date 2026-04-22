using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Identity;

/// <summary>
/// System-level administrative identity within the control-system access-control boundary.
/// Represents service accounts, admin operators, and system agents — not end-user identities
/// (those belong to trust-system).
/// </summary>
public sealed class IdentityAggregate : AggregateRoot
{
    public IdentityId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IdentityKind Kind { get; private set; }
    public IdentityStatus Status { get; private set; }

    private IdentityAggregate() { }

    public static IdentityAggregate Register(IdentityId id, string name, IdentityKind kind)
    {
        Guard.Against(string.IsNullOrEmpty(name), IdentityErrors.IdentityNameMustNotBeEmpty().Message);

        var aggregate = new IdentityAggregate();
        aggregate.RaiseDomainEvent(new IdentityRegisteredEvent(id, name, kind));
        return aggregate;
    }

    public void Suspend(string reason)
    {
        Guard.Against(Status == IdentityStatus.Suspended, IdentityErrors.IdentityAlreadySuspended().Message);
        Guard.Against(Status == IdentityStatus.Deactivated, IdentityErrors.CannotReactivatePermanentlyDeactivatedIdentity().Message);

        RaiseDomainEvent(new IdentitySuspendedEvent(Id, reason));
    }

    public void Deactivate()
    {
        Guard.Against(Status == IdentityStatus.Deactivated, IdentityErrors.IdentityAlreadyDeactivated().Message);

        RaiseDomainEvent(new IdentityDeactivatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case IdentityRegisteredEvent e:
                Id = e.Id;
                Name = e.Name;
                Kind = e.Kind;
                Status = IdentityStatus.Active;
                break;
            case IdentitySuspendedEvent:
                Status = IdentityStatus.Suspended;
                break;
            case IdentityDeactivatedEvent:
                Status = IdentityStatus.Deactivated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "Identity must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(Name), "Identity must have a non-empty Name.");
    }
}

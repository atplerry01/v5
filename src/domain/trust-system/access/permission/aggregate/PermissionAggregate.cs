using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Permission;

public sealed class PermissionAggregate : AggregateRoot
{
    public PermissionId Id { get; private set; }
    public PermissionDescriptor Descriptor { get; private set; }
    public PermissionStatus Status { get; private set; }

    private PermissionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static PermissionAggregate Define(
        PermissionId id,
        PermissionDescriptor descriptor)
    {
        var aggregate = new PermissionAggregate();
        aggregate.RaiseDomainEvent(new PermissionDefinedEvent(id, descriptor));
        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PermissionErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new PermissionActivatedEvent(Id));
    }

    // ── Deprecate ────────────────────────────────────────────────

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PermissionErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new PermissionDeprecatedEvent(Id));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PermissionDefinedEvent e:
                Id = e.PermissionId;
                Descriptor = e.Descriptor;
                Status = PermissionStatus.Defined;
                break;
            case PermissionActivatedEvent:
                Status = PermissionStatus.Active;
                break;
            case PermissionDeprecatedEvent:
                Status = PermissionStatus.Deprecated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw PermissionErrors.MissingId();

        if (Descriptor == default)
            throw PermissionErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw PermissionErrors.InvalidStateTransition(Status, "validate");
    }
}

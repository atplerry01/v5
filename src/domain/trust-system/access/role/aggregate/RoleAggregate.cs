using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Role;

public sealed class RoleAggregate : AggregateRoot
{
    public RoleId RoleId { get; private set; }
    public RoleDescriptor Descriptor { get; private set; }
    public RoleStatus Status { get; private set; }

    private RoleAggregate() { }

    public static RoleAggregate Define(RoleId id, RoleDescriptor descriptor, Timestamp definedAt)
    {
        var aggregate = new RoleAggregate();
        aggregate.RaiseDomainEvent(new RoleDefinedEvent(id, descriptor, definedAt));
        return aggregate;
    }

    public void Activate()
    {
        if (Status != RoleStatus.Defined)
            throw new DomainInvariantViolationException("Role can only be activated from Defined status.");
        RaiseDomainEvent(new RoleActivatedEvent(RoleId));
    }

    public void Deprecate()
    {
        if (Status != RoleStatus.Active)
            throw new DomainInvariantViolationException("Role can only be deprecated from Active status.");
        RaiseDomainEvent(new RoleDeprecatedEvent(RoleId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RoleDefinedEvent e:
                RoleId = e.RoleId;
                Descriptor = e.Descriptor;
                Status = RoleStatus.Defined;
                break;
            case RoleActivatedEvent:
                Status = RoleStatus.Active;
                break;
            case RoleDeprecatedEvent:
                Status = RoleStatus.Deprecated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(RoleId == default, "Role identity must be established.");
        Guard.Against(Descriptor == default, "Role descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Role status is not a defined enum value.");
    }
}

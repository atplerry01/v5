using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Principal;

public sealed class PrincipalAggregate : AggregateRoot
{
    public PrincipalId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PrincipalKind Kind { get; private set; }
    public string IdentityId { get; private set; } = string.Empty;
    public IReadOnlySet<string> RoleIds { get; private set; } = new HashSet<string>();
    public PrincipalStatus Status { get; private set; }

    private PrincipalAggregate() { }

    public static PrincipalAggregate Register(
        PrincipalId id,
        string name,
        PrincipalKind kind,
        string identityId)
    {
        Guard.Against(string.IsNullOrEmpty(name), PrincipalErrors.PrincipalNameMustNotBeEmpty().Message);

        var aggregate = new PrincipalAggregate();
        aggregate.RaiseDomainEvent(new PrincipalRegisteredEvent(id, name, kind, identityId));
        return aggregate;
    }

    public void AssignRole(string roleId)
    {
        Guard.Against(string.IsNullOrEmpty(roleId), PrincipalErrors.RoleIdMustNotBeEmpty().Message);
        Guard.Against(Status == PrincipalStatus.Deactivated, PrincipalErrors.CannotModifyDeactivatedPrincipal().Message);
        Guard.Against(RoleIds.Contains(roleId), PrincipalErrors.RoleAlreadyAssigned(roleId).Message);

        RaiseDomainEvent(new PrincipalRoleAssignedEvent(Id, roleId));
    }

    public void Deactivate()
    {
        Guard.Against(Status == PrincipalStatus.Deactivated, PrincipalErrors.PrincipalAlreadyDeactivated().Message);

        RaiseDomainEvent(new PrincipalDeactivatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PrincipalRegisteredEvent e:
                Id = e.Id;
                Name = e.Name;
                Kind = e.Kind;
                IdentityId = e.IdentityId;
                Status = PrincipalStatus.Active;
                break;
            case PrincipalRoleAssignedEvent e:
                RoleIds = new HashSet<string>(RoleIds) { e.RoleId };
                break;
            case PrincipalDeactivatedEvent:
                Status = PrincipalStatus.Deactivated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "Principal must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(Name), "Principal must have a non-empty Name.");
    }
}

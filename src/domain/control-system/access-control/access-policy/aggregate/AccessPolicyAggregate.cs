using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;

public sealed class AccessPolicyAggregate : AggregateRoot
{
    public AccessPolicyId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;
    public IReadOnlySet<string> AllowedRoleIds { get; private set; } = new HashSet<string>();
    public AccessPolicyStatus Status { get; private set; }

    private AccessPolicyAggregate() { }

    public static AccessPolicyAggregate Define(
        AccessPolicyId id,
        string name,
        string scope,
        IEnumerable<string> allowedRoleIds)
    {
        Guard.Against(string.IsNullOrEmpty(name), AccessPolicyErrors.PolicyNameMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(scope), AccessPolicyErrors.PolicyScopeMustNotBeEmpty().Message);

        var aggregate = new AccessPolicyAggregate();
        aggregate.RaiseDomainEvent(new AccessPolicyDefinedEvent(id, name, scope, allowedRoleIds.ToHashSet()));
        return aggregate;
    }

    public void Activate()
    {
        Guard.Against(Status == AccessPolicyStatus.Active, AccessPolicyErrors.PolicyAlreadyActive().Message);
        Guard.Against(Status != AccessPolicyStatus.Draft, AccessPolicyErrors.PolicyMustBeDraftBeforeActivation().Message);

        RaiseDomainEvent(new AccessPolicyActivatedEvent(Id));
    }

    public void Retire()
    {
        Guard.Against(Status == AccessPolicyStatus.Retired, AccessPolicyErrors.PolicyAlreadyRetired().Message);

        RaiseDomainEvent(new AccessPolicyRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AccessPolicyDefinedEvent e:
                Id = e.Id;
                Name = e.Name;
                Scope = e.Scope;
                AllowedRoleIds = e.AllowedRoleIds;
                Status = AccessPolicyStatus.Draft;
                break;
            case AccessPolicyActivatedEvent:
                Status = AccessPolicyStatus.Active;
                break;
            case AccessPolicyRetiredEvent:
                Status = AccessPolicyStatus.Retired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "AccessPolicy must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(Name), "AccessPolicy must have a non-empty Name.");
        Guard.Against(string.IsNullOrEmpty(Scope), "AccessPolicy must have a non-empty Scope.");
    }
}

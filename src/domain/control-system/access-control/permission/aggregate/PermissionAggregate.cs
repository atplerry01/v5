using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Permission;

public sealed class PermissionAggregate : AggregateRoot
{
    public PermissionId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ResourceScope { get; private set; } = string.Empty;
    public ActionMask Actions { get; private set; }
    public bool IsDeprecated { get; private set; }

    private PermissionAggregate() { }

    public static PermissionAggregate Define(PermissionId id, string name, string resourceScope, ActionMask actions)
    {
        Guard.Against(string.IsNullOrEmpty(name), "Permission name must not be empty.");
        Guard.Against(string.IsNullOrEmpty(resourceScope), "Permission resourceScope must not be empty.");
        Guard.Against(actions == ActionMask.None, "Permission must declare at least one action.");

        var aggregate = new PermissionAggregate();
        aggregate.RaiseDomainEvent(new PermissionDefinedEvent(id, name, resourceScope, actions));
        return aggregate;
    }

    public void Deprecate()
    {
        Guard.Against(IsDeprecated, "Permission is already deprecated.");
        RaiseDomainEvent(new PermissionDeprecatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PermissionDefinedEvent e:
                Id = e.Id; Name = e.Name; ResourceScope = e.ResourceScope; Actions = e.Actions;
                break;
            case PermissionDeprecatedEvent:
                IsDeprecated = true;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "Permission must have a non-empty Id.");
    }
}

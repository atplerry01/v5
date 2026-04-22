using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;

public sealed class PolicyDefinitionAggregate : AggregateRoot
{
    public PolicyId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public new int Version { get; private set; }
    public PolicyScope Scope { get; private set; } = null!;
    public PolicyDefinitionStatus Status { get; private set; }

    private PolicyDefinitionAggregate() { }

    public static PolicyDefinitionAggregate Define(
        PolicyId id,
        string name,
        PolicyScope scope)
    {
        Guard.Against(string.IsNullOrEmpty(name), "Policy name must not be empty.");

        var aggregate = new PolicyDefinitionAggregate();
        aggregate.RaiseDomainEvent(new PolicyDefinedEvent(id, name, scope, 1));
        return aggregate;
    }

    public void Deprecate()
    {
        Guard.Against(Status == PolicyDefinitionStatus.Deprecated,
            "Policy definition is already deprecated.");

        RaiseDomainEvent(new PolicyDeprecatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PolicyDefinedEvent e:
                Id = e.Id;
                Name = e.Name;
                Scope = e.Scope;
                Version = e.Version;
                Status = PolicyDefinitionStatus.Published;
                break;

            case PolicyDeprecatedEvent e:
                Status = PolicyDefinitionStatus.Deprecated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "PolicyDefinition must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(Name), "PolicyDefinition must have a non-empty Name.");
        Guard.Against(Scope is null, "PolicyDefinition must have a Scope.");
    }
}

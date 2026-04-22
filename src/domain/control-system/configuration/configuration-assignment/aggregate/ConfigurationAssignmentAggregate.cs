using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;

public sealed class ConfigurationAssignmentAggregate : AggregateRoot
{
    public ConfigurationAssignmentId Id { get; private set; }
    public string DefinitionId { get; private set; } = string.Empty;
    public string ScopeId { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public AssignmentStatus Status { get; private set; }

    private ConfigurationAssignmentAggregate() { }

    public static ConfigurationAssignmentAggregate Assign(
        ConfigurationAssignmentId id,
        string definitionId,
        string scopeId,
        string value)
    {
        Guard.Against(string.IsNullOrEmpty(definitionId), ConfigurationAssignmentErrors.DefinitionIdMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(scopeId), ConfigurationAssignmentErrors.ScopeIdMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(value), ConfigurationAssignmentErrors.ValueMustNotBeEmpty().Message);

        var aggregate = new ConfigurationAssignmentAggregate();
        aggregate.RaiseDomainEvent(new ConfigurationAssignedEvent(id, definitionId, scopeId, value));
        return aggregate;
    }

    public void Revoke()
    {
        Guard.Against(Status == AssignmentStatus.Revoked, ConfigurationAssignmentErrors.AssignmentAlreadyRevoked().Message);

        RaiseDomainEvent(new ConfigurationAssignmentRevokedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ConfigurationAssignedEvent e:
                Id = e.Id;
                DefinitionId = e.DefinitionId;
                ScopeId = e.ScopeId;
                Value = e.Value;
                Status = AssignmentStatus.Active;
                break;
            case ConfigurationAssignmentRevokedEvent:
                Status = AssignmentStatus.Revoked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "ConfigurationAssignment must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(DefinitionId), "ConfigurationAssignment must have a non-empty DefinitionId.");
        Guard.Against(string.IsNullOrEmpty(ScopeId), "ConfigurationAssignment must have a non-empty ScopeId.");
    }
}

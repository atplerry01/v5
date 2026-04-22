using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;

public sealed class ConfigurationStateAggregate : AggregateRoot
{
    public ConfigurationStateId Id { get; private set; }
    public string DefinitionId { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public new int Version { get; private set; }
    public bool IsRevoked { get; private set; }

    private ConfigurationStateAggregate() { }

    public static ConfigurationStateAggregate Set(
        ConfigurationStateId id,
        string definitionId,
        string value,
        int version)
    {
        Guard.Against(string.IsNullOrEmpty(definitionId), "ConfigurationState requires a definitionId.");
        Guard.Against(version < 1, "ConfigurationState version must be >= 1.");

        var aggregate = new ConfigurationStateAggregate();
        aggregate.RaiseDomainEvent(new ConfigurationStateSetEvent(id, definitionId, value, version));
        return aggregate;
    }

    public void Revoke()
    {
        Guard.Against(IsRevoked, "ConfigurationState is already revoked.");
        RaiseDomainEvent(new ConfigurationStateRevokedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ConfigurationStateSetEvent e:
                Id = e.Id; DefinitionId = e.DefinitionId; Value = e.Value; Version = e.Version;
                break;
            case ConfigurationStateRevokedEvent:
                IsRevoked = true;
                break;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "ConfigurationState must have an Id.");
}

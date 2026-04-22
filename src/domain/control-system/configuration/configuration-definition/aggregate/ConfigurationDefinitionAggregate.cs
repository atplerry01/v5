using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;

public sealed class ConfigurationDefinitionAggregate : AggregateRoot
{
    public ConfigurationDefinitionId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ConfigValueType ValueType { get; private set; }
    public string? DefaultValue { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsDeprecated { get; private set; }

    private ConfigurationDefinitionAggregate() { }

    public static ConfigurationDefinitionAggregate Define(
        ConfigurationDefinitionId id,
        string name,
        ConfigValueType valueType,
        string description,
        string? defaultValue = null)
    {
        Guard.Against(string.IsNullOrEmpty(name), "Configuration name must not be empty.");
        Guard.Against(string.IsNullOrEmpty(description), "Configuration description must not be empty.");

        var aggregate = new ConfigurationDefinitionAggregate();
        aggregate.RaiseDomainEvent(new ConfigurationDefinedEvent(id, name, valueType, description, defaultValue));
        return aggregate;
    }

    public void Deprecate()
    {
        Guard.Against(IsDeprecated, "ConfigurationDefinition is already deprecated.");
        RaiseDomainEvent(new ConfigurationDefinitionDeprecatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ConfigurationDefinedEvent e:
                Id = e.Id; Name = e.Name; ValueType = e.ValueType; Description = e.Description; DefaultValue = e.DefaultValue;
                break;
            case ConfigurationDefinitionDeprecatedEvent:
                IsDeprecated = true;
                break;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "ConfigurationDefinition must have an Id.");
}

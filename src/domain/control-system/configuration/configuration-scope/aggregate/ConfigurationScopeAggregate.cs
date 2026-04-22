using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;

public sealed class ConfigurationScopeAggregate : AggregateRoot
{
    public ConfigurationScopeId Id { get; private set; }
    public string DefinitionId { get; private set; } = string.Empty;
    public string Classification { get; private set; } = string.Empty;
    public string? Context { get; private set; }
    public bool IsRemoved { get; private set; }

    private ConfigurationScopeAggregate() { }

    public static ConfigurationScopeAggregate Declare(
        ConfigurationScopeId id,
        string definitionId,
        string classification,
        string? context = null)
    {
        Guard.Against(string.IsNullOrEmpty(definitionId), "ConfigurationScope requires a definitionId.");
        Guard.Against(string.IsNullOrEmpty(classification), "ConfigurationScope requires a classification.");

        var aggregate = new ConfigurationScopeAggregate();
        aggregate.RaiseDomainEvent(new ConfigurationScopeDeclaredEvent(id, definitionId, classification, context));
        return aggregate;
    }

    public void Remove()
    {
        Guard.Against(IsRemoved, "ConfigurationScope is already removed.");
        RaiseDomainEvent(new ConfigurationScopeRemovedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ConfigurationScopeDeclaredEvent e:
                Id = e.Id; DefinitionId = e.DefinitionId; Classification = e.Classification; Context = e.Context;
                break;
            case ConfigurationScopeRemovedEvent:
                IsRemoved = true;
                break;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "ConfigurationScope must have an Id.");
}

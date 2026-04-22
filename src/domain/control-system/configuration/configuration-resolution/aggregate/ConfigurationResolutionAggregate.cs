using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationResolution;

public sealed class ConfigurationResolutionAggregate : AggregateRoot
{
    public ConfigurationResolutionId Id { get; private set; }
    public string DefinitionId { get; private set; } = string.Empty;
    public string ScopeId { get; private set; } = string.Empty;
    public string StateId { get; private set; } = string.Empty;
    public string ResolvedValue { get; private set; } = string.Empty;
    public DateTimeOffset ResolvedAt { get; private set; }

    private ConfigurationResolutionAggregate() { }

    public static ConfigurationResolutionAggregate Record(
        ConfigurationResolutionId id,
        string definitionId,
        string scopeId,
        string stateId,
        string resolvedValue,
        DateTimeOffset resolvedAt)
    {
        Guard.Against(string.IsNullOrEmpty(definitionId), "ConfigurationResolution requires a definitionId.");
        Guard.Against(string.IsNullOrEmpty(scopeId), "ConfigurationResolution requires a scopeId.");
        Guard.Against(string.IsNullOrEmpty(stateId), "ConfigurationResolution requires a stateId.");

        var aggregate = new ConfigurationResolutionAggregate();
        aggregate.RaiseDomainEvent(new ConfigurationResolvedEvent(
            id, definitionId, scopeId, stateId, resolvedValue, resolvedAt));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        if (domainEvent is ConfigurationResolvedEvent e)
        {
            Id = e.Id; DefinitionId = e.DefinitionId; ScopeId = e.ScopeId;
            StateId = e.StateId; ResolvedValue = e.ResolvedValue; ResolvedAt = e.ResolvedAt;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "ConfigurationResolution must have an Id.");
}

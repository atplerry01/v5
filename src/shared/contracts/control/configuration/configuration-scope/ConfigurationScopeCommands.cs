using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;

public sealed record DeclareConfigurationScopeCommand(
    Guid ScopeId,
    string DefinitionId,
    string Classification,
    string? Context = null) : IHasAggregateId
{
    public Guid AggregateId => ScopeId;
}

public sealed record RemoveConfigurationScopeCommand(
    Guid ScopeId) : IHasAggregateId
{
    public Guid AggregateId => ScopeId;
}

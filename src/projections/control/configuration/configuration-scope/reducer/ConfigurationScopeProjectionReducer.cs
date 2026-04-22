using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationScope;

namespace Whycespace.Projections.Control.Configuration.ConfigurationScope.Reducer;

public static class ConfigurationScopeProjectionReducer
{
    public static ConfigurationScopeReadModel Apply(ConfigurationScopeReadModel state, ConfigurationScopeDeclaredEventSchema e) =>
        state with
        {
            ScopeId        = e.AggregateId,
            DefinitionId   = e.DefinitionId,
            Classification = e.Classification,
            Context        = e.Context,
            IsRemoved      = false
        };

    public static ConfigurationScopeReadModel Apply(ConfigurationScopeReadModel state, ConfigurationScopeRemovedEventSchema e) =>
        state with { IsRemoved = true };
}

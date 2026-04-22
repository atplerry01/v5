using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationResolution;

namespace Whycespace.Projections.Control.Configuration.ConfigurationResolution.Reducer;

public static class ConfigurationResolutionProjectionReducer
{
    public static ConfigurationResolutionReadModel Apply(ConfigurationResolutionReadModel state, ConfigurationResolvedEventSchema e) =>
        state with
        {
            ResolutionId  = e.AggregateId,
            DefinitionId  = e.DefinitionId,
            ScopeId       = e.ScopeId,
            StateId       = e.StateId,
            ResolvedValue = e.ResolvedValue,
            ResolvedAt    = e.ResolvedAt
        };
}

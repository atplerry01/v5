using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationState;

namespace Whycespace.Projections.Control.Configuration.ConfigurationState.Reducer;

public static class ConfigurationStateProjectionReducer
{
    public static ConfigurationStateReadModel Apply(ConfigurationStateReadModel state, ConfigurationStateSetEventSchema e) =>
        state with
        {
            StateId      = e.AggregateId,
            DefinitionId = e.DefinitionId,
            Value        = e.Value,
            Version      = e.Version,
            IsRevoked    = false
        };

    public static ConfigurationStateReadModel Apply(ConfigurationStateReadModel state, ConfigurationStateRevokedEventSchema e) =>
        state with { IsRevoked = true };
}

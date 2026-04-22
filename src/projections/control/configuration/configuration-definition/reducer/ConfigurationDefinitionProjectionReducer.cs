using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationDefinition;

namespace Whycespace.Projections.Control.Configuration.ConfigurationDefinition.Reducer;

public static class ConfigurationDefinitionProjectionReducer
{
    public static ConfigurationDefinitionReadModel Apply(ConfigurationDefinitionReadModel state, ConfigurationDefinedEventSchema e) =>
        state with
        {
            DefinitionId = e.AggregateId,
            Name         = e.Name,
            ValueType    = e.ValueType,
            Description  = e.Description,
            DefaultValue = e.DefaultValue,
            IsDeprecated = false
        };

    public static ConfigurationDefinitionReadModel Apply(ConfigurationDefinitionReadModel state, ConfigurationDefinitionDeprecatedEventSchema e) =>
        state with { IsDeprecated = true };
}

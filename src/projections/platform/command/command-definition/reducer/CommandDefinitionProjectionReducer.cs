using Whycespace.Shared.Contracts.Events.Platform.Command.CommandDefinition;
using Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;

namespace Whycespace.Projections.Platform.Command.CommandDefinition.Reducer;

public static class CommandDefinitionProjectionReducer
{
    public static CommandDefinitionReadModel Apply(CommandDefinitionReadModel state, CommandDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            CommandDefinitionId = e.AggregateId,
            TypeName = e.TypeName,
            Version = e.Version,
            SchemaId = e.SchemaId,
            OwnerClassification = e.OwnerClassification,
            OwnerContext = e.OwnerContext,
            OwnerDomain = e.OwnerDomain,
            Status = "Active",
            LastModifiedAt = at
        };

    public static CommandDefinitionReadModel Apply(CommandDefinitionReadModel state, CommandDeprecatedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Deprecated", LastModifiedAt = at };
}

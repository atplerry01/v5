using Whycespace.Shared.Contracts.Events.Platform.Schema.SchemaDefinition;
using Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;

namespace Whycespace.Projections.Platform.Schema.SchemaDefinition.Reducer;

public static class SchemaDefinitionProjectionReducer
{
    public static SchemaDefinitionReadModel Apply(SchemaDefinitionReadModel state, SchemaDefinitionDraftedEventSchema e, DateTimeOffset at) =>
        state with
        {
            SchemaDefinitionId = e.AggregateId,
            SchemaName = e.SchemaName,
            Version = e.Version,
            CompatibilityMode = e.CompatibilityMode,
            Status = "Draft",
            LastModifiedAt = at
        };

    public static SchemaDefinitionReadModel Apply(SchemaDefinitionReadModel state, SchemaDefinitionPublishedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Published", LastModifiedAt = at };

    public static SchemaDefinitionReadModel Apply(SchemaDefinitionReadModel state, SchemaDefinitionDeprecatedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Deprecated", LastModifiedAt = at };
}

using Whycespace.Shared.Contracts.Events.Platform.Event.EventDefinition;
using Whycespace.Shared.Contracts.Platform.Event.EventDefinition;

namespace Whycespace.Projections.Platform.Event.EventDefinition.Reducer;

public static class EventDefinitionProjectionReducer
{
    public static EventDefinitionReadModel Apply(EventDefinitionReadModel state, EventDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            EventDefinitionId = e.AggregateId,
            TypeName = e.TypeName,
            Version = e.Version,
            SchemaId = e.SchemaId,
            SourceClassification = e.SourceClassification,
            SourceContext = e.SourceContext,
            SourceDomain = e.SourceDomain,
            Status = "Active",
            LastModifiedAt = at
        };

    public static EventDefinitionReadModel Apply(EventDefinitionReadModel state, EventDefinitionDeprecatedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Deprecated", LastModifiedAt = at };
}

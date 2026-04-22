namespace Whycespace.Shared.Contracts.Events.Platform.Event.EventSchema;

public sealed record EventSchemaRegisteredEventSchema(
    Guid AggregateId,
    string Name,
    string Version,
    string CompatibilityMode);

public sealed record EventSchemaDeprecatedEventSchema(Guid AggregateId);

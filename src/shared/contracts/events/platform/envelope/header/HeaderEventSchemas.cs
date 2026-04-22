namespace Whycespace.Shared.Contracts.Events.Platform.Envelope.Header;

public sealed record HeaderSchemaRegisteredEventSchema(
    Guid AggregateId,
    string HeaderKind,
    int SchemaVersion,
    IReadOnlyList<string> RequiredFields);

public sealed record HeaderSchemaDeprecatedEventSchema(Guid AggregateId);

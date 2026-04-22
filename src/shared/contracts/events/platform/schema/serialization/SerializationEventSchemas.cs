namespace Whycespace.Shared.Contracts.Events.Platform.Schema.Serialization;

public sealed record SerializationFormatRegisteredEventSchema(
    Guid AggregateId,
    string FormatName,
    string Encoding,
    Guid? SchemaRef,
    string RoundTripGuarantee,
    int FormatVersion);

public sealed record SerializationFormatDeprecatedEventSchema(Guid AggregateId);

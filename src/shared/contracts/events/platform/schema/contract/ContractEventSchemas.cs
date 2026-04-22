namespace Whycespace.Shared.Contracts.Events.Platform.Schema.Contract;

public sealed record ContractRegisteredEventSchema(
    Guid AggregateId,
    string ContractName,
    string PublisherClassification,
    string PublisherContext,
    string PublisherDomain,
    Guid SchemaRef,
    int SchemaVersion);

public sealed record ContractSubscriberAddedEventSchema(
    Guid AggregateId,
    string SubscriberClassification,
    string SubscriberContext,
    string SubscriberDomain,
    int MinSchemaVersion,
    string RequiredCompatibilityMode);

public sealed record ContractDeprecatedEventSchema(Guid AggregateId);

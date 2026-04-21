namespace Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Document;

public sealed record DocumentCreatedEventSchema(
    Guid AggregateId,
    string Title,
    string Type,
    string Classification,
    Guid StructuralOwnerId,
    string BusinessOwnerKind,
    Guid BusinessOwnerId,
    DateTimeOffset CreatedAt);

public sealed record DocumentMetadataUpdatedEventSchema(
    Guid AggregateId,
    string OldTitle,
    string NewTitle,
    string OldType,
    string NewType,
    string OldClassification,
    string NewClassification,
    DateTimeOffset UpdatedAt);

public sealed record DocumentVersionAttachedEventSchema(
    Guid AggregateId,
    Guid VersionId,
    DateTimeOffset AttachedAt);

public sealed record DocumentActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record DocumentArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);

public sealed record DocumentRestoredEventSchema(
    Guid AggregateId,
    DateTimeOffset RestoredAt);

public sealed record DocumentSupersededEventSchema(
    Guid AggregateId,
    Guid SupersedingDocumentId,
    DateTimeOffset SupersededAt);

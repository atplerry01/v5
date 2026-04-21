namespace Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Asset;

public sealed record AssetCreatedEventSchema(
    Guid AggregateId,
    string Title,
    string Classification,
    DateTimeOffset CreatedAt);

public sealed record AssetRenamedEventSchema(
    Guid AggregateId,
    string PreviousTitle,
    string NewTitle,
    DateTimeOffset RenamedAt);

public sealed record AssetReclassifiedEventSchema(
    Guid AggregateId,
    string PreviousClassification,
    string NewClassification,
    DateTimeOffset ReclassifiedAt);

public sealed record AssetActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record AssetRetiredEventSchema(
    Guid AggregateId,
    DateTimeOffset RetiredAt);

public sealed record AssetKindAssignedEventSchema(
    Guid AggregateId,
    string PreviousKind,
    string NewKind,
    DateTimeOffset AssignedAt);

namespace Whycespace.Shared.Contracts.Events.Content.Media.LifecycleChange.Version;

public sealed record MediaVersionCreatedEventSchema(
    Guid AggregateId,
    Guid AssetRef,
    int VersionMajor,
    int VersionMinor,
    Guid FileRef,
    Guid? PreviousVersionId,
    DateTimeOffset CreatedAt);

public sealed record MediaVersionActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record MediaVersionSupersededEventSchema(
    Guid AggregateId,
    Guid SuccessorVersionId,
    DateTimeOffset SupersededAt);

public sealed record MediaVersionWithdrawnEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset WithdrawnAt);

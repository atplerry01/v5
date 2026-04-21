namespace Whycespace.Shared.Contracts.Events.Content.Document.LifecycleChange.Version;

public sealed record DocumentVersionCreatedEventSchema(
    Guid AggregateId,
    Guid DocumentRef,
    int Major,
    int Minor,
    Guid ArtifactRef,
    Guid? PreviousVersionId,
    DateTimeOffset CreatedAt);

public sealed record DocumentVersionActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record DocumentVersionSupersededEventSchema(
    Guid AggregateId,
    Guid SuccessorVersionId,
    DateTimeOffset SupersededAt);

public sealed record DocumentVersionWithdrawnEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset WithdrawnAt);

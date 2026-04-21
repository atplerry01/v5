namespace Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Availability;

public sealed record PlaybackCreatedEventSchema(
    Guid AggregateId,
    Guid SourceId,
    string SourceKind,
    string Mode,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableUntil,
    DateTimeOffset CreatedAt);

public sealed record PlaybackEnabledEventSchema(
    Guid AggregateId,
    DateTimeOffset EnabledAt);

public sealed record PlaybackDisabledEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset DisabledAt);

public sealed record PlaybackWindowUpdatedEventSchema(
    Guid AggregateId,
    DateTimeOffset PreviousAvailableFrom,
    DateTimeOffset PreviousAvailableUntil,
    DateTimeOffset NewAvailableFrom,
    DateTimeOffset NewAvailableUntil,
    DateTimeOffset UpdatedAt);

public sealed record PlaybackArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);

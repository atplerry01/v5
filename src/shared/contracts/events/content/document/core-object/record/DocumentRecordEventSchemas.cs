namespace Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Record;

public sealed record DocumentRecordCreatedEventSchema(
    Guid AggregateId,
    Guid DocumentId,
    DateTimeOffset CreatedAt);

public sealed record DocumentRecordLockedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset LockedAt);

public sealed record DocumentRecordUnlockedEventSchema(
    Guid AggregateId,
    DateTimeOffset UnlockedAt);

public sealed record DocumentRecordClosedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset ClosedAt);

public sealed record DocumentRecordArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);

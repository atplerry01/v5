using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.Record;

public sealed record CreateDocumentRecordCommand(
    Guid RecordId,
    Guid DocumentId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => RecordId;
}

public sealed record LockDocumentRecordCommand(
    Guid RecordId,
    string Reason,
    DateTimeOffset LockedAt) : IHasAggregateId
{
    public Guid AggregateId => RecordId;
}

public sealed record UnlockDocumentRecordCommand(
    Guid RecordId,
    DateTimeOffset UnlockedAt) : IHasAggregateId
{
    public Guid AggregateId => RecordId;
}

public sealed record CloseDocumentRecordCommand(
    Guid RecordId,
    string Reason,
    DateTimeOffset ClosedAt) : IHasAggregateId
{
    public Guid AggregateId => RecordId;
}

public sealed record ArchiveDocumentRecordCommand(
    Guid RecordId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => RecordId;
}

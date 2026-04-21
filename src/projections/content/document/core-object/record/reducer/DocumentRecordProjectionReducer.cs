using Whycespace.Shared.Contracts.Content.Document.CoreObject.Record;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Record;

namespace Whycespace.Projections.Content.Document.CoreObject.Record.Reducer;

public static class DocumentRecordProjectionReducer
{
    public static DocumentRecordReadModel Apply(DocumentRecordReadModel state, DocumentRecordCreatedEventSchema e) =>
        state with
        {
            RecordId = e.AggregateId,
            DocumentId = e.DocumentId,
            Status = "Open",
            ClosureReason = string.Empty,
            CreatedAt = e.CreatedAt,
            ClosedAt = null,
            LastModifiedAt = e.CreatedAt
        };

    public static DocumentRecordReadModel Apply(DocumentRecordReadModel state, DocumentRecordLockedEventSchema e) =>
        state with
        {
            RecordId = e.AggregateId,
            Status = "Locked",
            LastModifiedAt = e.LockedAt
        };

    public static DocumentRecordReadModel Apply(DocumentRecordReadModel state, DocumentRecordUnlockedEventSchema e) =>
        state with
        {
            RecordId = e.AggregateId,
            Status = "Open",
            LastModifiedAt = e.UnlockedAt
        };

    public static DocumentRecordReadModel Apply(DocumentRecordReadModel state, DocumentRecordClosedEventSchema e) =>
        state with
        {
            RecordId = e.AggregateId,
            Status = "Closed",
            ClosureReason = e.Reason,
            ClosedAt = e.ClosedAt,
            LastModifiedAt = e.ClosedAt
        };

    public static DocumentRecordReadModel Apply(DocumentRecordReadModel state, DocumentRecordArchivedEventSchema e) =>
        state with
        {
            RecordId = e.AggregateId,
            Status = "Archived",
            LastModifiedAt = e.ArchivedAt
        };
}

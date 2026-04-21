using Whycespace.Shared.Contracts.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.File;

namespace Whycespace.Projections.Content.Document.CoreObject.File.Reducer;

public static class DocumentFileProjectionReducer
{
    public static DocumentFileReadModel Apply(DocumentFileReadModel state, DocumentFileRegisteredEventSchema e) =>
        state with
        {
            DocumentFileId = e.AggregateId,
            DocumentId = e.DocumentId,
            StorageRef = e.StorageRef,
            DeclaredChecksum = e.DeclaredChecksum,
            MimeType = e.MimeType,
            SizeBytes = e.SizeBytes,
            IntegrityStatus = "Unverified",
            Status = "Registered",
            SuccessorFileId = null,
            RegisteredAt = e.RegisteredAt,
            LastModifiedAt = e.RegisteredAt
        };

    public static DocumentFileReadModel Apply(DocumentFileReadModel state, DocumentFileIntegrityVerifiedEventSchema e) =>
        state with
        {
            DocumentFileId = e.AggregateId,
            IntegrityStatus = "Verified",
            LastModifiedAt = e.VerifiedAt
        };

    public static DocumentFileReadModel Apply(DocumentFileReadModel state, DocumentFileSupersededEventSchema e) =>
        state with
        {
            DocumentFileId = e.AggregateId,
            Status = "Superseded",
            SuccessorFileId = e.SuccessorFileId,
            LastModifiedAt = e.SupersededAt
        };

    public static DocumentFileReadModel Apply(DocumentFileReadModel state, DocumentFileArchivedEventSchema e) =>
        state with
        {
            DocumentFileId = e.AggregateId,
            Status = "Archived",
            LastModifiedAt = e.ArchivedAt
        };
}

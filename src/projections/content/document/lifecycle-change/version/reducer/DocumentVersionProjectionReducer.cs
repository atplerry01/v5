using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Events.Content.Document.LifecycleChange.Version;

namespace Whycespace.Projections.Content.Document.LifecycleChange.Version.Reducer;

public static class DocumentVersionProjectionReducer
{
    public static DocumentVersionReadModel Apply(DocumentVersionReadModel state, DocumentVersionCreatedEventSchema e) =>
        state with
        {
            VersionId = e.AggregateId,
            DocumentRef = e.DocumentRef,
            Major = e.Major,
            Minor = e.Minor,
            ArtifactRef = e.ArtifactRef,
            PreviousVersionId = e.PreviousVersionId,
            Status = "Draft",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static DocumentVersionReadModel Apply(DocumentVersionReadModel state, DocumentVersionActivatedEventSchema e) =>
        state with
        {
            VersionId = e.AggregateId,
            Status = "Active",
            LastModifiedAt = e.ActivatedAt
        };

    public static DocumentVersionReadModel Apply(DocumentVersionReadModel state, DocumentVersionSupersededEventSchema e) =>
        state with
        {
            VersionId = e.AggregateId,
            SuccessorVersionId = e.SuccessorVersionId,
            Status = "Superseded",
            LastModifiedAt = e.SupersededAt
        };

    public static DocumentVersionReadModel Apply(DocumentVersionReadModel state, DocumentVersionWithdrawnEventSchema e) =>
        state with
        {
            VersionId = e.AggregateId,
            Status = "Withdrawn",
            WithdrawalReason = e.Reason,
            LastModifiedAt = e.WithdrawnAt
        };
}

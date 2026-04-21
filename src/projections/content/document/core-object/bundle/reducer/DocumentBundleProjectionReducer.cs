using Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Bundle;

namespace Whycespace.Projections.Content.Document.CoreObject.Bundle.Reducer;

public static class DocumentBundleProjectionReducer
{
    public static DocumentBundleReadModel Apply(DocumentBundleReadModel state, DocumentBundleCreatedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Name = e.Name,
            Status = "Open",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt,
            FinalizedAt = null
        };

    public static DocumentBundleReadModel Apply(DocumentBundleReadModel state, DocumentBundleRenamedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Name = e.NewName,
            LastModifiedAt = e.RenamedAt
        };

    public static DocumentBundleReadModel Apply(DocumentBundleReadModel state, DocumentBundleMemberAddedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            LastModifiedAt = e.AddedAt
        };

    public static DocumentBundleReadModel Apply(DocumentBundleReadModel state, DocumentBundleMemberRemovedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            LastModifiedAt = e.RemovedAt
        };

    public static DocumentBundleReadModel Apply(DocumentBundleReadModel state, DocumentBundleFinalizedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Status = "Finalized",
            FinalizedAt = e.FinalizedAt,
            LastModifiedAt = e.FinalizedAt
        };

    public static DocumentBundleReadModel Apply(DocumentBundleReadModel state, DocumentBundleArchivedEventSchema e) =>
        state with
        {
            BundleId = e.AggregateId,
            Status = "Archived",
            LastModifiedAt = e.ArchivedAt
        };
}

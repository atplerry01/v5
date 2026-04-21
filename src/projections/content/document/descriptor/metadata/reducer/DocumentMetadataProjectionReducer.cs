using Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Events.Content.Document.Descriptor.Metadata;

namespace Whycespace.Projections.Content.Document.Descriptor.Metadata.Reducer;

public static class DocumentMetadataProjectionReducer
{
    public static DocumentMetadataReadModel Apply(DocumentMetadataReadModel state, DocumentMetadataCreatedEventSchema e) =>
        state with
        {
            MetadataId = e.AggregateId,
            DocumentId = e.DocumentId,
            Status = "Open",
            Entries = new Dictionary<string, string>(),
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt,
            FinalizedAt = null
        };

    public static DocumentMetadataReadModel Apply(DocumentMetadataReadModel state, DocumentMetadataEntryAddedEventSchema e)
    {
        var entries = new Dictionary<string, string>(state.Entries)
        {
            [e.Key] = e.Value
        };
        return state with
        {
            MetadataId = e.AggregateId,
            Entries = entries,
            LastModifiedAt = e.AddedAt
        };
    }

    public static DocumentMetadataReadModel Apply(DocumentMetadataReadModel state, DocumentMetadataEntryUpdatedEventSchema e)
    {
        var entries = new Dictionary<string, string>(state.Entries)
        {
            [e.Key] = e.NewValue
        };
        return state with
        {
            MetadataId = e.AggregateId,
            Entries = entries,
            LastModifiedAt = e.UpdatedAt
        };
    }

    public static DocumentMetadataReadModel Apply(DocumentMetadataReadModel state, DocumentMetadataEntryRemovedEventSchema e)
    {
        var entries = new Dictionary<string, string>(state.Entries);
        entries.Remove(e.Key);
        return state with
        {
            MetadataId = e.AggregateId,
            Entries = entries,
            LastModifiedAt = e.RemovedAt
        };
    }

    public static DocumentMetadataReadModel Apply(DocumentMetadataReadModel state, DocumentMetadataFinalizedEventSchema e) =>
        state with
        {
            MetadataId = e.AggregateId,
            Status = "Finalized",
            FinalizedAt = e.FinalizedAt,
            LastModifiedAt = e.FinalizedAt
        };
}

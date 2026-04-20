using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed class DocumentMetadataAggregate : AggregateRoot
{
    private readonly Dictionary<MetadataKey, MetadataEntry> _entries = new();

    public DocumentMetadataId MetadataId { get; private set; }
    public DocumentRef DocumentRef { get; private set; }
    public MetadataStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }

    public IReadOnlyDictionary<MetadataKey, MetadataEntry> Entries => _entries;

    private DocumentMetadataAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentMetadataAggregate Create(
        DocumentMetadataId metadataId,
        DocumentRef documentRef,
        Timestamp createdAt)
    {
        var aggregate = new DocumentMetadataAggregate();

        aggregate.RaiseDomainEvent(new DocumentMetadataCreatedEvent(
            metadataId,
            documentRef,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void AddEntry(MetadataKey key, MetadataValue value, Timestamp addedAt)
    {
        EnsureMutable();

        if (_entries.ContainsKey(key))
            throw DocumentMetadataErrors.DuplicateKey(key.Value);

        RaiseDomainEvent(new DocumentMetadataEntryAddedEvent(MetadataId, key, value, addedAt));
    }

    public void UpdateEntry(MetadataKey key, MetadataValue newValue, Timestamp updatedAt)
    {
        EnsureMutable();

        if (!_entries.TryGetValue(key, out var existing))
            throw DocumentMetadataErrors.UnknownKey(key.Value);

        if (existing.Value == newValue)
            return;

        RaiseDomainEvent(new DocumentMetadataEntryUpdatedEvent(
            MetadataId,
            key,
            existing.Value,
            newValue,
            updatedAt));
    }

    public void RemoveEntry(MetadataKey key, Timestamp removedAt)
    {
        EnsureMutable();

        if (!_entries.ContainsKey(key))
            throw DocumentMetadataErrors.UnknownKey(key.Value);

        RaiseDomainEvent(new DocumentMetadataEntryRemovedEvent(MetadataId, key, removedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == MetadataStatus.Finalized)
            throw DocumentMetadataErrors.MetadataAlreadyFinalized();

        if (_entries.Count == 0)
            throw DocumentMetadataErrors.EmptyMetadata();

        RaiseDomainEvent(new DocumentMetadataFinalizedEvent(MetadataId, finalizedAt));
    }

    private void EnsureMutable()
    {
        if (Status == MetadataStatus.Finalized)
            throw DocumentMetadataErrors.MetadataAlreadyFinalized();
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentMetadataCreatedEvent e:
                MetadataId = e.MetadataId;
                DocumentRef = e.DocumentRef;
                Status = MetadataStatus.Open;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case DocumentMetadataEntryAddedEvent e:
                _entries[e.Key] = new MetadataEntry(e.Key, e.Value, e.AddedAt);
                LastModifiedAt = e.AddedAt;
                break;

            case DocumentMetadataEntryUpdatedEvent e:
                _entries[e.Key] = new MetadataEntry(e.Key, e.NewValue, e.UpdatedAt);
                LastModifiedAt = e.UpdatedAt;
                break;

            case DocumentMetadataEntryRemovedEvent e:
                _entries.Remove(e.Key);
                LastModifiedAt = e.RemovedAt;
                break;

            case DocumentMetadataFinalizedEvent e:
                Status = MetadataStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DocumentRef.Value == Guid.Empty)
            throw DocumentMetadataErrors.OrphanedMetadata();
    }
}

using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed class MediaMetadataAggregate : AggregateRoot
{
    private readonly Dictionary<MediaMetadataKey, MediaMetadataEntry> _entries = new();

    public MediaMetadataId MetadataId { get; private set; }
    public MediaAssetRef AssetRef { get; private set; }
    public MediaMetadataStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }

    public IReadOnlyDictionary<MediaMetadataKey, MediaMetadataEntry> Entries => _entries;

    private MediaMetadataAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static MediaMetadataAggregate Create(
        MediaMetadataId metadataId,
        MediaAssetRef assetRef,
        Timestamp createdAt)
    {
        var aggregate = new MediaMetadataAggregate();

        aggregate.RaiseDomainEvent(new MediaMetadataCreatedEvent(metadataId, assetRef, createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void AddEntry(MediaMetadataKey key, MediaMetadataValue value, Timestamp addedAt)
    {
        EnsureMutable();

        if (_entries.ContainsKey(key))
            throw MediaMetadataErrors.DuplicateKey(key.Value);

        RaiseDomainEvent(new MediaMetadataEntryAddedEvent(MetadataId, key, value, addedAt));
    }

    public void UpdateEntry(MediaMetadataKey key, MediaMetadataValue newValue, Timestamp updatedAt)
    {
        EnsureMutable();

        if (!_entries.TryGetValue(key, out var existing))
            throw MediaMetadataErrors.UnknownKey(key.Value);

        if (existing.Value == newValue)
            return;

        RaiseDomainEvent(new MediaMetadataEntryUpdatedEvent(
            MetadataId,
            key,
            existing.Value,
            newValue,
            updatedAt));
    }

    public void RemoveEntry(MediaMetadataKey key, Timestamp removedAt)
    {
        EnsureMutable();

        if (!_entries.ContainsKey(key))
            throw MediaMetadataErrors.UnknownKey(key.Value);

        RaiseDomainEvent(new MediaMetadataEntryRemovedEvent(MetadataId, key, removedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == MediaMetadataStatus.Finalized)
            throw MediaMetadataErrors.MetadataAlreadyFinalized();

        if (_entries.Count == 0)
            throw MediaMetadataErrors.EmptyMetadata();

        RaiseDomainEvent(new MediaMetadataFinalizedEvent(MetadataId, finalizedAt));
    }

    private void EnsureMutable()
    {
        if (Status == MediaMetadataStatus.Finalized)
            throw MediaMetadataErrors.MetadataAlreadyFinalized();
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MediaMetadataCreatedEvent e:
                MetadataId = e.MetadataId;
                AssetRef = e.AssetRef;
                Status = MediaMetadataStatus.Open;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case MediaMetadataEntryAddedEvent e:
                _entries[e.Key] = new MediaMetadataEntry(e.Key, e.Value, e.AddedAt);
                LastModifiedAt = e.AddedAt;
                break;

            case MediaMetadataEntryUpdatedEvent e:
                _entries[e.Key] = new MediaMetadataEntry(e.Key, e.NewValue, e.UpdatedAt);
                LastModifiedAt = e.UpdatedAt;
                break;

            case MediaMetadataEntryRemovedEvent e:
                _entries.Remove(e.Key);
                LastModifiedAt = e.RemovedAt;
                break;

            case MediaMetadataFinalizedEvent e:
                Status = MediaMetadataStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (AssetRef.Value == Guid.Empty)
            throw MediaMetadataErrors.OrphanedMetadata();
    }
}

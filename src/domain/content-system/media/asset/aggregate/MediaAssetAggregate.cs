using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed class MediaAssetAggregate : AggregateRoot
{
    private static readonly MediaAssetRegistrationSpecification RegistrationSpec = new();
    private static readonly MediaAssetTransitionSpecification TransitionSpec = new();

    private readonly List<MediaTag> _tags = new();
    private bool _reachedAvailable;

    public MediaAssetId MediaAssetId { get; private set; }
    public string OwnerRef { get; private set; } = string.Empty;
    public MediaType MediaType { get; private set; }
    public MediaTitle Title { get; private set; } = default!;
    public MediaDescription Description { get; private set; } = MediaDescription.Empty;
    public ContentDigest Digest { get; private set; } = default!;
    public StorageLocation Storage { get; private set; } = default!;
    public MediaAssetStatus Status { get; private set; }
    public Timestamp RegisteredAt { get; private set; }
    public Timestamp? LastTransitionedAt { get; private set; }
    public IReadOnlyList<MediaTag> Tags => _tags.AsReadOnly();

    private MediaAssetAggregate() { }

    public static MediaAssetAggregate Register(
        EventId eventId,
        AggregateId aggregateId,
        CorrelationId correlationId,
        CausationId causationId,
        MediaAssetId assetId,
        string ownerRef,
        MediaType mediaType,
        MediaTitle title,
        MediaDescription description,
        ContentDigest digest,
        StorageLocation storage,
        Timestamp registeredAt)
    {
        var candidate = new MediaAssetRegistrationCandidate(
            assetId, ownerRef, title, description, mediaType, digest, storage);
        RegistrationSpec.EnsureSatisfied(candidate);

        var aggregate = new MediaAssetAggregate();
        aggregate.RaiseDomainEvent(new MediaAssetRegisteredEvent(
            eventId,
            aggregateId,
            correlationId,
            causationId,
            assetId,
            ownerRef,
            mediaType,
            title.Value,
            description.Value,
            digest.Value,
            storage.Uri,
            storage.SizeBytes,
            registeredAt));
        return aggregate;
    }

    public void StartProcessing(
        EventId eventId,
        AggregateId aggregateId,
        CorrelationId correlationId,
        CausationId causationId,
        Timestamp startedAt)
    {
        if (Status != MediaAssetStatus.Draft)
            throw MediaAssetErrors.CannotProcessFromStatus(Status);
        TransitionSpec.EnsureSatisfied(Status, MediaAssetStatus.Processing);

        RaiseDomainEvent(new MediaAssetProcessingStartedEvent(
            eventId, aggregateId, correlationId, causationId, MediaAssetId, startedAt));
    }

    public void MarkAvailable(
        EventId eventId,
        AggregateId aggregateId,
        CorrelationId correlationId,
        CausationId causationId,
        Timestamp availableAt)
    {
        if (Status != MediaAssetStatus.Processing)
            throw MediaAssetErrors.CannotMarkAvailableFromStatus(Status);
        TransitionSpec.EnsureSatisfied(Status, MediaAssetStatus.Available);

        RaiseDomainEvent(new MediaAssetAvailableEvent(
            eventId, aggregateId, correlationId, causationId, MediaAssetId, availableAt));
    }

    public void Publish(
        EventId eventId,
        AggregateId aggregateId,
        CorrelationId correlationId,
        CausationId causationId,
        Timestamp publishedAt)
    {
        if (Status == MediaAssetStatus.Published)
            throw MediaAssetErrors.AssetAlreadyPublished();
        if (Status != MediaAssetStatus.Available)
            throw MediaAssetErrors.CannotPublishFromStatus(Status);
        TransitionSpec.EnsureSatisfied(Status, MediaAssetStatus.Published);

        RaiseDomainEvent(new MediaAssetPublishedEvent(
            eventId, aggregateId, correlationId, causationId, MediaAssetId, publishedAt));
    }

    public void Unpublish(
        EventId eventId,
        AggregateId aggregateId,
        CorrelationId correlationId,
        CausationId causationId,
        Timestamp unpublishedAt)
    {
        if (Status != MediaAssetStatus.Published)
            throw MediaAssetErrors.AssetNotPublished();
        TransitionSpec.EnsureSatisfied(Status, MediaAssetStatus.Available);

        RaiseDomainEvent(new MediaAssetUnpublishedEvent(
            eventId, aggregateId, correlationId, causationId, MediaAssetId, unpublishedAt));
    }

    public void Archive(
        EventId eventId,
        AggregateId aggregateId,
        CorrelationId correlationId,
        CausationId causationId,
        Timestamp archivedAt)
    {
        if (Status == MediaAssetStatus.Archived)
            throw MediaAssetErrors.CannotArchiveFromStatus(Status);
        TransitionSpec.EnsureSatisfied(Status, MediaAssetStatus.Archived);

        RaiseDomainEvent(new MediaAssetArchivedEvent(
            eventId, aggregateId, correlationId, causationId, MediaAssetId, archivedAt));
    }

    public void UpdateMetadata(
        EventId eventId,
        AggregateId aggregateId,
        CorrelationId correlationId,
        CausationId causationId,
        MediaTitle title,
        MediaDescription description,
        IReadOnlyCollection<MediaTag> tags,
        Timestamp updatedAt)
    {
        if (Status == MediaAssetStatus.Archived)
            throw MediaAssetErrors.AssetArchived();
        Guard.Against(tags is null, MediaAssetErrors.InvalidTag().Message);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tag in tags!)
        {
            if (!seen.Add(tag.Value))
                throw MediaAssetErrors.DuplicateTag(tag.Value);
        }

        var tagValues = tags.Select(t => t.Value).ToList().AsReadOnly();
        RaiseDomainEvent(new MediaAssetMetadataUpdatedEvent(
            eventId,
            aggregateId,
            correlationId,
            causationId,
            MediaAssetId,
            title.Value,
            description.Value,
            tagValues,
            updatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MediaAssetRegisteredEvent e:
                MediaAssetId = e.MediaAssetId;
                OwnerRef = e.OwnerRef;
                MediaType = e.MediaType;
                Title = MediaTitle.Create(e.Title);
                Description = MediaDescription.Create(e.Description);
                Digest = ContentDigest.Create(e.ContentDigest);
                Storage = StorageLocation.Create(e.StorageUri, e.StorageSizeBytes);
                Status = MediaAssetStatus.Draft;
                RegisteredAt = e.RegisteredAt;
                break;

            case MediaAssetProcessingStartedEvent e:
                Status = MediaAssetStatus.Processing;
                LastTransitionedAt = e.StartedAt;
                break;

            case MediaAssetAvailableEvent e:
                Status = MediaAssetStatus.Available;
                _reachedAvailable = true;
                LastTransitionedAt = e.AvailableAt;
                break;

            case MediaAssetPublishedEvent e:
                Status = MediaAssetStatus.Published;
                LastTransitionedAt = e.PublishedAt;
                break;

            case MediaAssetUnpublishedEvent e:
                Status = MediaAssetStatus.Available;
                LastTransitionedAt = e.UnpublishedAt;
                break;

            case MediaAssetArchivedEvent e:
                Status = MediaAssetStatus.Archived;
                LastTransitionedAt = e.ArchivedAt;
                break;

            case MediaAssetMetadataUpdatedEvent e:
                Title = MediaTitle.Create(e.Title);
                Description = MediaDescription.Create(e.Description);
                _tags.Clear();
                foreach (var t in e.Tags)
                    _tags.Add(MediaTag.Create(t));
                LastTransitionedAt = e.UpdatedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Title is null) return;

        if (string.IsNullOrWhiteSpace(Title.Value))
            throw MediaAssetErrors.TitleMissing();
        if (Digest is null || string.IsNullOrWhiteSpace(Digest.Value))
            throw MediaAssetErrors.DigestMissing();
        if (Storage is null || string.IsNullOrWhiteSpace(Storage.Uri))
            throw MediaAssetErrors.StorageMissing();

        if (Status == MediaAssetStatus.Published && !_reachedAvailable)
            throw MediaAssetErrors.PublishedWithoutAvailability();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tag in _tags)
        {
            if (!seen.Add(tag.Value))
                throw MediaAssetErrors.DuplicateTag(tag.Value);
        }
    }
}

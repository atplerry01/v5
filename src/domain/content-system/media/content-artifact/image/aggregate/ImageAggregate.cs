using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public sealed class ImageAggregate : AggregateRoot
{
    public ImageId ImageId { get; private set; }
    public MediaAssetRef AssetRef { get; private set; }
    public MediaFileRef? FileRef { get; private set; }
    public ImageDimensions Dimensions { get; private set; }
    public ImageOrientation Orientation { get; private set; }
    public ImageStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private ImageAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ImageAggregate Create(
        ImageId imageId,
        MediaAssetRef assetRef,
        MediaFileRef? fileRef,
        ImageDimensions dimensions,
        ImageOrientation orientation,
        Timestamp createdAt)
    {
        var aggregate = new ImageAggregate();

        aggregate.RaiseDomainEvent(new ImageCreatedEvent(
            imageId,
            assetRef,
            fileRef,
            dimensions,
            orientation,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(
        ImageDimensions newDimensions,
        ImageOrientation newOrientation,
        Timestamp updatedAt)
    {
        if (Status == ImageStatus.Archived)
            throw ImageErrors.ImageArchived();

        if (Dimensions == newDimensions && Orientation == newOrientation)
            return;

        RaiseDomainEvent(new ImageUpdatedEvent(ImageId, newDimensions, newOrientation, updatedAt));
    }

    public void Activate(Timestamp activatedAt)
    {
        if (Status == ImageStatus.Archived)
            throw ImageErrors.ImageArchived();

        if (Status == ImageStatus.Active)
            throw ImageErrors.AlreadyActive();

        RaiseDomainEvent(new ImageActivatedEvent(ImageId, activatedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == ImageStatus.Archived)
            throw ImageErrors.AlreadyArchived();

        RaiseDomainEvent(new ImageArchivedEvent(ImageId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ImageCreatedEvent e:
                ImageId = e.ImageId;
                AssetRef = e.AssetRef;
                FileRef = e.FileRef;
                Dimensions = e.Dimensions;
                Orientation = e.Orientation;
                Status = ImageStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case ImageUpdatedEvent e:
                Dimensions = e.Dimensions;
                Orientation = e.Orientation;
                LastModifiedAt = e.UpdatedAt;
                break;

            case ImageActivatedEvent e:
                Status = ImageStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case ImageArchivedEvent e:
                Status = ImageStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (AssetRef.Value == Guid.Empty)
            throw ImageErrors.OrphanedImage();
    }
}

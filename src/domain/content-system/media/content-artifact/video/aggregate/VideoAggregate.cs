using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public sealed class VideoAggregate : AggregateRoot
{
    public VideoId VideoId { get; private set; }
    public MediaAssetRef AssetRef { get; private set; }
    public MediaFileRef? FileRef { get; private set; }
    public VideoDimensions Dimensions { get; private set; }
    public VideoDuration Duration { get; private set; }
    public FrameRate FrameRate { get; private set; }
    public VideoStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private VideoAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static VideoAggregate Create(
        VideoId videoId,
        MediaAssetRef assetRef,
        MediaFileRef? fileRef,
        VideoDimensions dimensions,
        VideoDuration duration,
        FrameRate frameRate,
        Timestamp createdAt)
    {
        var aggregate = new VideoAggregate();

        aggregate.RaiseDomainEvent(new VideoCreatedEvent(
            videoId,
            assetRef,
            fileRef,
            dimensions,
            duration,
            frameRate,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(
        VideoDimensions newDimensions,
        VideoDuration newDuration,
        FrameRate newFrameRate,
        Timestamp updatedAt)
    {
        if (Status == VideoStatus.Archived)
            throw VideoErrors.VideoArchived();

        if (Dimensions == newDimensions && Duration == newDuration && FrameRate == newFrameRate)
            return;

        RaiseDomainEvent(new VideoUpdatedEvent(VideoId, newDimensions, newDuration, newFrameRate, updatedAt));
    }

    public void Activate(Timestamp activatedAt)
    {
        if (Status == VideoStatus.Archived)
            throw VideoErrors.VideoArchived();

        if (Status == VideoStatus.Active)
            throw VideoErrors.AlreadyActive();

        RaiseDomainEvent(new VideoActivatedEvent(VideoId, activatedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == VideoStatus.Archived)
            throw VideoErrors.AlreadyArchived();

        RaiseDomainEvent(new VideoArchivedEvent(VideoId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case VideoCreatedEvent e:
                VideoId = e.VideoId;
                AssetRef = e.AssetRef;
                FileRef = e.FileRef;
                Dimensions = e.Dimensions;
                Duration = e.Duration;
                FrameRate = e.FrameRate;
                Status = VideoStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case VideoUpdatedEvent e:
                Dimensions = e.Dimensions;
                Duration = e.Duration;
                FrameRate = e.FrameRate;
                LastModifiedAt = e.UpdatedAt;
                break;

            case VideoActivatedEvent e:
                Status = VideoStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case VideoArchivedEvent e:
                Status = VideoStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (AssetRef.Value == Guid.Empty)
            throw VideoErrors.OrphanedVideo();
    }
}

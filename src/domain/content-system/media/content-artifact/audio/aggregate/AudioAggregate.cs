using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public sealed class AudioAggregate : AggregateRoot
{
    public AudioId AudioId { get; private set; }
    public MediaAssetRef AssetRef { get; private set; }
    public MediaFileRef? FileRef { get; private set; }
    public AudioFormat Format { get; private set; }
    public AudioDuration Duration { get; private set; }
    public ChannelCount Channels { get; private set; }
    public AudioStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private AudioAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static AudioAggregate Create(
        AudioId audioId,
        MediaAssetRef assetRef,
        MediaFileRef? fileRef,
        AudioFormat format,
        AudioDuration duration,
        ChannelCount channels,
        Timestamp createdAt)
    {
        var aggregate = new AudioAggregate();

        aggregate.RaiseDomainEvent(new AudioCreatedEvent(
            audioId,
            assetRef,
            fileRef,
            format,
            duration,
            channels,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(
        AudioFormat newFormat,
        AudioDuration newDuration,
        ChannelCount newChannels,
        Timestamp updatedAt)
    {
        if (Status == AudioStatus.Archived)
            throw AudioErrors.AudioArchived();

        if (Format == newFormat && Duration == newDuration && Channels == newChannels)
            return;

        RaiseDomainEvent(new AudioUpdatedEvent(AudioId, newFormat, newDuration, newChannels, updatedAt));
    }

    public void Activate(Timestamp activatedAt)
    {
        if (Status == AudioStatus.Archived)
            throw AudioErrors.AudioArchived();

        if (Status == AudioStatus.Active)
            throw AudioErrors.AlreadyActive();

        RaiseDomainEvent(new AudioActivatedEvent(AudioId, activatedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == AudioStatus.Archived)
            throw AudioErrors.AlreadyArchived();

        RaiseDomainEvent(new AudioArchivedEvent(AudioId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AudioCreatedEvent e:
                AudioId = e.AudioId;
                AssetRef = e.AssetRef;
                FileRef = e.FileRef;
                Format = e.Format;
                Duration = e.Duration;
                Channels = e.Channels;
                Status = AudioStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case AudioUpdatedEvent e:
                Format = e.Format;
                Duration = e.Duration;
                Channels = e.Channels;
                LastModifiedAt = e.UpdatedAt;
                break;

            case AudioActivatedEvent e:
                Status = AudioStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case AudioArchivedEvent e:
                Status = AudioStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (AssetRef.Value == Guid.Empty)
            throw AudioErrors.OrphanedAudio();
    }
}

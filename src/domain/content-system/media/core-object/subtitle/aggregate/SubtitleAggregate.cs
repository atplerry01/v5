using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed class SubtitleAggregate : AggregateRoot
{
    public SubtitleId SubtitleId { get; private set; }
    public MediaAssetRef AssetRef { get; private set; }
    public MediaFileRef? SourceFileRef { get; private set; }
    public SubtitleFormat Format { get; private set; }
    public SubtitleLanguage Language { get; private set; }
    public SubtitleWindow? Window { get; private set; }
    public SubtitleOutputRef? OutputRef { get; private set; }
    public SubtitleStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }

    private SubtitleAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static SubtitleAggregate Create(
        SubtitleId subtitleId,
        MediaAssetRef assetRef,
        MediaFileRef? sourceFileRef,
        SubtitleFormat format,
        SubtitleLanguage language,
        SubtitleWindow? window,
        Timestamp createdAt)
    {
        var aggregate = new SubtitleAggregate();

        aggregate.RaiseDomainEvent(new SubtitleCreatedEvent(
            subtitleId,
            assetRef,
            sourceFileRef,
            format,
            language,
            window,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(SubtitleOutputRef outputRef, Timestamp updatedAt)
    {
        if (Status == SubtitleStatus.Finalized)
            throw SubtitleErrors.SubtitleFinalized();

        if (Status == SubtitleStatus.Archived)
            throw SubtitleErrors.SubtitleArchived();

        RaiseDomainEvent(new SubtitleUpdatedEvent(SubtitleId, outputRef, updatedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == SubtitleStatus.Finalized)
            throw SubtitleErrors.AlreadyFinalized();

        if (Status == SubtitleStatus.Archived)
            throw SubtitleErrors.SubtitleArchived();

        RaiseDomainEvent(new SubtitleFinalizedEvent(SubtitleId, finalizedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == SubtitleStatus.Archived)
            throw SubtitleErrors.AlreadyArchived();

        RaiseDomainEvent(new SubtitleArchivedEvent(SubtitleId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SubtitleCreatedEvent e:
                SubtitleId = e.SubtitleId;
                AssetRef = e.AssetRef;
                SourceFileRef = e.SourceFileRef;
                Format = e.Format;
                Language = e.Language;
                Window = e.Window;
                Status = SubtitleStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case SubtitleUpdatedEvent e:
                OutputRef = e.OutputRef;
                Status = SubtitleStatus.Active;
                LastModifiedAt = e.UpdatedAt;
                break;

            case SubtitleFinalizedEvent e:
                Status = SubtitleStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;

            case SubtitleArchivedEvent e:
                Status = SubtitleStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (AssetRef.Value == Guid.Empty)
            throw SubtitleErrors.OrphanedSubtitle();
    }
}

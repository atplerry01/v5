using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed class TranscriptAggregate : AggregateRoot
{
    public TranscriptId TranscriptId { get; private set; }
    public MediaAssetRef AssetRef { get; private set; }
    public MediaFileRef? SourceFileRef { get; private set; }
    public TranscriptFormat Format { get; private set; }
    public TranscriptLanguage Language { get; private set; }
    public TranscriptOutputRef? OutputRef { get; private set; }
    public TranscriptStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }

    private TranscriptAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static TranscriptAggregate Create(
        TranscriptId transcriptId,
        MediaAssetRef assetRef,
        MediaFileRef? sourceFileRef,
        TranscriptFormat format,
        TranscriptLanguage language,
        Timestamp createdAt)
    {
        var aggregate = new TranscriptAggregate();

        aggregate.RaiseDomainEvent(new TranscriptCreatedEvent(
            transcriptId,
            assetRef,
            sourceFileRef,
            format,
            language,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(TranscriptOutputRef outputRef, Timestamp updatedAt)
    {
        if (Status == TranscriptStatus.Finalized)
            throw TranscriptErrors.TranscriptFinalized();

        if (Status == TranscriptStatus.Archived)
            throw TranscriptErrors.TranscriptArchived();

        RaiseDomainEvent(new TranscriptUpdatedEvent(TranscriptId, outputRef, updatedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == TranscriptStatus.Finalized)
            throw TranscriptErrors.AlreadyFinalized();

        if (Status == TranscriptStatus.Archived)
            throw TranscriptErrors.TranscriptArchived();

        RaiseDomainEvent(new TranscriptFinalizedEvent(TranscriptId, finalizedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == TranscriptStatus.Archived)
            throw TranscriptErrors.AlreadyArchived();

        RaiseDomainEvent(new TranscriptArchivedEvent(TranscriptId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TranscriptCreatedEvent e:
                TranscriptId = e.TranscriptId;
                AssetRef = e.AssetRef;
                SourceFileRef = e.SourceFileRef;
                Format = e.Format;
                Language = e.Language;
                Status = TranscriptStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case TranscriptUpdatedEvent e:
                OutputRef = e.OutputRef;
                Status = TranscriptStatus.Active;
                LastModifiedAt = e.UpdatedAt;
                break;

            case TranscriptFinalizedEvent e:
                Status = TranscriptStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;

            case TranscriptArchivedEvent e:
                Status = TranscriptStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (AssetRef.Value == Guid.Empty)
            throw TranscriptErrors.OrphanedTranscript();
    }
}

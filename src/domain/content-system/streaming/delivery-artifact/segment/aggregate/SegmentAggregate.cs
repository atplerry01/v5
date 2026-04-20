using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public sealed class SegmentAggregate : AggregateRoot
{
    public SegmentId SegmentId { get; private set; }
    public SegmentSourceRef SourceRef { get; private set; }
    public SegmentSequenceNumber Sequence { get; private set; }
    public SegmentWindow Window { get; private set; }
    public SegmentStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp? PublishedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private SegmentAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static SegmentAggregate Create(
        SegmentId segmentId,
        SegmentSourceRef sourceRef,
        SegmentSequenceNumber sequence,
        SegmentWindow window,
        Timestamp createdAt)
    {
        var aggregate = new SegmentAggregate();

        aggregate.RaiseDomainEvent(new SegmentCreatedEvent(
            segmentId,
            sourceRef,
            sequence,
            window,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Publish(Timestamp publishedAt)
    {
        if (Status == SegmentStatus.Archived)
            throw SegmentErrors.SegmentArchived();

        if (Status == SegmentStatus.Retired)
            throw SegmentErrors.SegmentRetired();

        if (Status == SegmentStatus.Published)
            throw SegmentErrors.AlreadyPublished();

        RaiseDomainEvent(new SegmentPublishedEvent(SegmentId, publishedAt));
    }

    public void Retire(Timestamp retiredAt)
    {
        if (Status == SegmentStatus.Archived)
            throw SegmentErrors.SegmentArchived();

        if (Status == SegmentStatus.Retired)
            throw SegmentErrors.AlreadyRetired();

        RaiseDomainEvent(new SegmentRetiredEvent(SegmentId, retiredAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == SegmentStatus.Archived)
            throw SegmentErrors.AlreadyArchived();

        RaiseDomainEvent(new SegmentArchivedEvent(SegmentId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SegmentCreatedEvent e:
                SegmentId = e.SegmentId;
                SourceRef = e.SourceRef;
                Sequence = e.Sequence;
                Window = e.Window;
                Status = SegmentStatus.Created;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case SegmentPublishedEvent e:
                Status = SegmentStatus.Published;
                PublishedAt ??= e.PublishedAt;
                LastModifiedAt = e.PublishedAt;
                break;

            case SegmentRetiredEvent e:
                Status = SegmentStatus.Retired;
                LastModifiedAt = e.RetiredAt;
                break;

            case SegmentArchivedEvent e:
                Status = SegmentStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (SourceRef.Value == Guid.Empty)
            throw SegmentErrors.OrphanedSegment();
    }
}

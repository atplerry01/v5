using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public sealed class MetricsAggregate : AggregateRoot
{
    public MetricsId MetricsId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public RecordingRef? RecordingRef { get; private set; }
    public MetricsWindow Window { get; private set; }
    public MetricsSnapshot Snapshot { get; private set; } = default!;
    public MetricsStatus Status { get; private set; }
    public Timestamp CapturedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private MetricsAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static MetricsAggregate Capture(
        MetricsId metricsId,
        StreamRef streamRef,
        RecordingRef? recordingRef,
        MetricsWindow window,
        MetricsSnapshot snapshot,
        Timestamp capturedAt)
    {
        var aggregate = new MetricsAggregate();

        aggregate.RaiseDomainEvent(new MetricsCapturedEvent(
            metricsId,
            streamRef,
            recordingRef,
            window,
            snapshot,
            capturedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(MetricsSnapshot newSnapshot, Timestamp updatedAt)
    {
        if (Status == MetricsStatus.Finalized)
            throw MetricsErrors.MetricsFinalized();

        if (Status == MetricsStatus.Archived)
            throw MetricsErrors.MetricsArchived();

        if (Snapshot == newSnapshot)
            return;

        RaiseDomainEvent(new MetricsUpdatedEvent(MetricsId, Snapshot, newSnapshot, updatedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == MetricsStatus.Finalized)
            throw MetricsErrors.AlreadyFinalized();

        if (Status == MetricsStatus.Archived)
            throw MetricsErrors.MetricsArchived();

        RaiseDomainEvent(new MetricsFinalizedEvent(MetricsId, finalizedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == MetricsStatus.Archived)
            throw MetricsErrors.AlreadyArchived();

        if (Status != MetricsStatus.Finalized)
            throw MetricsErrors.CannotArchiveUnlessFinalized();

        RaiseDomainEvent(new MetricsArchivedEvent(MetricsId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MetricsCapturedEvent e:
                MetricsId = e.MetricsId;
                StreamRef = e.StreamRef;
                RecordingRef = e.RecordingRef;
                Window = e.Window;
                Snapshot = e.Snapshot;
                Status = MetricsStatus.Capturing;
                CapturedAt = e.CapturedAt;
                LastModifiedAt = e.CapturedAt;
                break;

            case MetricsUpdatedEvent e:
                Snapshot = e.NewSnapshot;
                Status = MetricsStatus.Updated;
                LastModifiedAt = e.UpdatedAt;
                break;

            case MetricsFinalizedEvent e:
                Status = MetricsStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;

            case MetricsArchivedEvent e:
                Status = MetricsStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw MetricsErrors.OrphanedMetrics();
    }
}

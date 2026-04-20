using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed class ObservabilityAggregate : AggregateRoot
{
    public ObservabilityId ObservabilityId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public ArchiveRef? ArchiveRef { get; private set; }
    public ObservabilityWindow Window { get; private set; }
    public ObservabilitySnapshot Snapshot { get; private set; } = default!;
    public ObservabilityStatus Status { get; private set; }
    public Timestamp CapturedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private ObservabilityAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ObservabilityAggregate Capture(
        ObservabilityId metricsId,
        StreamRef streamRef,
        ArchiveRef? recordingRef,
        ObservabilityWindow window,
        ObservabilitySnapshot snapshot,
        Timestamp capturedAt)
    {
        var aggregate = new ObservabilityAggregate();

        aggregate.RaiseDomainEvent(new ObservabilityCapturedEvent(
            metricsId,
            streamRef,
            recordingRef,
            window,
            snapshot,
            capturedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(ObservabilitySnapshot newSnapshot, Timestamp updatedAt)
    {
        if (Status == ObservabilityStatus.Finalized)
            throw ObservabilityErrors.MetricsFinalized();

        if (Status == ObservabilityStatus.Archived)
            throw ObservabilityErrors.MetricsArchived();

        if (Snapshot == newSnapshot)
            return;

        RaiseDomainEvent(new ObservabilityUpdatedEvent(ObservabilityId, Snapshot, newSnapshot, updatedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == ObservabilityStatus.Finalized)
            throw ObservabilityErrors.AlreadyFinalized();

        if (Status == ObservabilityStatus.Archived)
            throw ObservabilityErrors.MetricsArchived();

        RaiseDomainEvent(new ObservabilityFinalizedEvent(ObservabilityId, finalizedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == ObservabilityStatus.Archived)
            throw ObservabilityErrors.AlreadyArchived();

        if (Status != ObservabilityStatus.Finalized)
            throw ObservabilityErrors.CannotArchiveUnlessFinalized();

        RaiseDomainEvent(new ObservabilityArchivedEvent(ObservabilityId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ObservabilityCapturedEvent e:
                ObservabilityId = e.ObservabilityId;
                StreamRef = e.StreamRef;
                ArchiveRef = e.ArchiveRef;
                Window = e.Window;
                Snapshot = e.Snapshot;
                Status = ObservabilityStatus.Capturing;
                CapturedAt = e.CapturedAt;
                LastModifiedAt = e.CapturedAt;
                break;

            case ObservabilityUpdatedEvent e:
                Snapshot = e.NewSnapshot;
                Status = ObservabilityStatus.Updated;
                LastModifiedAt = e.UpdatedAt;
                break;

            case ObservabilityFinalizedEvent e:
                Status = ObservabilityStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;

            case ObservabilityArchivedEvent e:
                Status = ObservabilityStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw ObservabilityErrors.OrphanedMetrics();
    }
}

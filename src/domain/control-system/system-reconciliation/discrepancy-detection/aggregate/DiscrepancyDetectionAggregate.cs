using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

public sealed class DiscrepancyDetectionAggregate : AggregateRoot
{
    public DiscrepancyDetectionId Id { get; private set; }
    public DiscrepancyKind Kind { get; private set; }
    public string SourceReference { get; private set; } = string.Empty;
    public DetectionStatus Status { get; private set; }
    public DateTimeOffset DetectedAt { get; private set; }

    private DiscrepancyDetectionAggregate() { }

    public static DiscrepancyDetectionAggregate Detect(
        DiscrepancyDetectionId id,
        DiscrepancyKind kind,
        string sourceReference,
        DateTimeOffset detectedAt)
    {
        Guard.Against(string.IsNullOrEmpty(sourceReference), DiscrepancyDetectionErrors.SourceReferenceMustNotBeEmpty().Message);

        var aggregate = new DiscrepancyDetectionAggregate();
        aggregate.RaiseDomainEvent(new DiscrepancyDetectedEvent(id, kind, sourceReference, detectedAt));
        return aggregate;
    }

    public void Dismiss(string reason, DateTimeOffset dismissedAt)
    {
        Guard.Against(
            Status != DetectionStatus.Detected,
            DiscrepancyDetectionErrors.DetectionAlreadyClosed(Status).Message);
        Guard.Against(string.IsNullOrEmpty(reason), DiscrepancyDetectionErrors.DismissalReasonMustNotBeEmpty().Message);

        RaiseDomainEvent(new DiscrepancyDetectionDismissedEvent(Id, reason, dismissedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DiscrepancyDetectedEvent e:
                Id = e.Id;
                Kind = e.Kind;
                SourceReference = e.SourceReference;
                Status = DetectionStatus.Detected;
                DetectedAt = e.DetectedAt;
                break;
            case DiscrepancyDetectionDismissedEvent:
                Status = DetectionStatus.Dismissed;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "DiscrepancyDetection must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(SourceReference), "DiscrepancyDetection must have a non-empty SourceReference.");
    }
}

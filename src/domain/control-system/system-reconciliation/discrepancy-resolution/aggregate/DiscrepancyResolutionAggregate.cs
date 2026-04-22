using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;

public sealed class DiscrepancyResolutionAggregate : AggregateRoot
{
    public DiscrepancyResolutionId Id { get; private set; }
    public DiscrepancyDetectionId DetectionId { get; private set; }
    public ResolutionStatus Status { get; private set; }
    public ResolutionOutcome? Outcome { get; private set; }
    public DateTimeOffset InitiatedAt { get; private set; }

    private DiscrepancyResolutionAggregate() { }

    public static DiscrepancyResolutionAggregate Initiate(
        DiscrepancyResolutionId id,
        DiscrepancyDetectionId detectionId,
        DateTimeOffset initiatedAt)
    {
        var aggregate = new DiscrepancyResolutionAggregate();
        aggregate.RaiseDomainEvent(new DiscrepancyResolutionInitiatedEvent(id, detectionId, initiatedAt));
        return aggregate;
    }

    public void Complete(ResolutionOutcome outcome, string notes, DateTimeOffset completedAt)
    {
        Guard.Against(
            Status != ResolutionStatus.Initiated,
            DiscrepancyResolutionErrors.ResolutionAlreadyTerminated(Status).Message);
        Guard.Against(string.IsNullOrEmpty(notes), DiscrepancyResolutionErrors.ResolutionNotesRequiredOnCompletion().Message);

        RaiseDomainEvent(new DiscrepancyResolutionCompletedEvent(Id, outcome, notes, completedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DiscrepancyResolutionInitiatedEvent e:
                Id = e.Id;
                DetectionId = e.DetectionId;
                Status = ResolutionStatus.Initiated;
                InitiatedAt = e.InitiatedAt;
                break;
            case DiscrepancyResolutionCompletedEvent e:
                Status = ResolutionStatus.Completed;
                Outcome = e.Outcome;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "DiscrepancyResolution must have a non-empty Id.");
        Guard.Against(DetectionId.Value is null, "DiscrepancyResolution must reference a non-empty detection ID.");
    }
}

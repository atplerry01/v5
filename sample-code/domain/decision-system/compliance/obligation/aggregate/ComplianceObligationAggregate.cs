namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

using Whycespace.Domain.DecisionSystem.Compliance.Obligation;
using Whycespace.Domain.SharedKernel;

public sealed class ComplianceObligationAggregate : AggregateRoot
{
    public Guid RegulationId { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public ObligationStatus Status { get; private set; } = default!;
    public ObligationDeadline Deadline { get; private set; } = default!;
    public Guid? AssignedEntityId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? FulfilledAt { get; private set; }

    private ComplianceObligationAggregate() { }

    public static ComplianceObligationAggregate Create(
        Guid obligationId,
        Guid regulationId,
        Guid jurisdictionId,
        string title,
        string description,
        ObligationDeadline deadline)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(ObligationErrors.InvalidTitle, "Obligation title is required.");

        var obligation = new ComplianceObligationAggregate();
        var @event = new ObligationCreatedEvent(
            obligationId,
            regulationId,
            jurisdictionId,
            title,
            description,
            deadline.DueDate,
            deadline.GracePeriodDays);

        obligation.Apply(@event);
        obligation.RaiseDomainEvent(@event);
        return obligation;
    }

    public void Assign(Guid entityId)
    {
        if (Status == ObligationStatus.Fulfilled || Status == ObligationStatus.Waived)
            throw new DomainException(ObligationErrors.InvalidTransition, "Cannot assign a completed obligation.");

        var @event = new ObligationAssignedEvent(Id, entityId);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void MarkInProgress()
    {
        if (Status != ObligationStatus.Pending && Status != ObligationStatus.Assigned)
            throw new DomainException(ObligationErrors.InvalidTransition, $"Cannot start obligation in '{Status.Value}' status.");

        var @event = new ObligationProgressedEvent(Id);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Fulfill()
    {
        if (Status != ObligationStatus.InProgress)
            throw new DomainException(ObligationErrors.InvalidTransition, "Only in-progress obligations can be fulfilled.");

        var @event = new ObligationFulfilledEvent(Id);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Breach(string reason)
    {
        if (Status == ObligationStatus.Fulfilled || Status == ObligationStatus.Waived)
            throw new DomainException(ObligationErrors.InvalidTransition, "Cannot breach a completed obligation.");

        var @event = new ObligationBreachedEvent(Id, reason);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Waive(string reason)
    {
        if (Status == ObligationStatus.Fulfilled || Status == ObligationStatus.Breached)
            throw new DomainException(ObligationErrors.InvalidTransition, $"Cannot waive obligation in '{Status.Value}' status.");

        var @event = new ObligationWaivedEvent(Id, reason);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public bool IsOverdue(DateTimeOffset asOf) =>
        Status != ObligationStatus.Fulfilled &&
        Status != ObligationStatus.Waived &&
        Deadline.IsOverdue(asOf);

    private void Apply(ObligationCreatedEvent @event)
    {
        Id = @event.ObligationId;
        RegulationId = @event.RegulationId;
        JurisdictionId = @event.JurisdictionId;
        Title = @event.Title;
        Description = @event.Description;
        Status = ObligationStatus.Pending;
        Deadline = new ObligationDeadline(@event.DueDate, @event.GracePeriodDays);
        CreatedAt = @event.OccurredAt;
    }

    private void Apply(ObligationAssignedEvent @event)
    {
        AssignedEntityId = @event.EntityId;
        Status = ObligationStatus.Assigned;
    }

    private void Apply(ObligationProgressedEvent _)
    {
        Status = ObligationStatus.InProgress;
    }

    private void Apply(ObligationFulfilledEvent @event)
    {
        Status = ObligationStatus.Fulfilled;
        FulfilledAt = @event.OccurredAt;
    }

    private void Apply(ObligationBreachedEvent _)
    {
        Status = ObligationStatus.Breached;
    }

    private void Apply(ObligationWaivedEvent _)
    {
        Status = ObligationStatus.Waived;
    }
}

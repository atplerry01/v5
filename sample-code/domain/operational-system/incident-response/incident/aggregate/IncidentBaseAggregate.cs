using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentBaseAggregate : AggregateRoot
{
    private readonly List<IncidentTimelineEntry> _timeline = [];
    private readonly List<IncidentEscalation> _escalations = [];

    public IncidentId IncidentId { get; private set; }
    public IncidentType IncidentType { get; private set; } = default!;
    public IncidentSeverity Severity { get; private set; } = default!;
    public IncidentPriority Priority { get; private set; } = default!;
    public IncidentStatus Status { get; private set; } = default!;
    public IncidentSource Source { get; private set; } = default!;
    public IncidentSLA SLA { get; private set; } = default!;
    public IncidentReference Reference { get; private set; } = IncidentReference.None;
    public IncidentCorrelationId SourceCorrelationId { get; private set; }
    public Guid AffectedEntityId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public IncidentAssignment? CurrentAssignment { get; private set; }
    public int EscalationLevel { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    public IReadOnlyList<IncidentTimelineEntry> Timeline => _timeline.AsReadOnly();
    public IReadOnlyList<IncidentEscalation> Escalations => _escalations.AsReadOnly();

    public static IncidentBaseAggregate Create(
        Guid incidentId,
        IncidentType type,
        IncidentSeverity severity,
        IncidentSource source,
        Guid affectedEntityId,
        string description,
        IncidentReference reference,
        IncidentCorrelationId sourceCorrelationId = default)
    {
        Guard.AgainstDefault(incidentId);
        Guard.AgainstNull(type);
        Guard.AgainstNull(severity);
        Guard.AgainstNull(source);
        Guard.AgainstDefault(affectedEntityId);
        Guard.AgainstEmpty(description);

        var priority = IncidentPriority.FromSeverity(severity);

        var incident = new IncidentBaseAggregate();
        incident.Apply(new IncidentCreatedEvent(
            incidentId, type.Value, severity.Value, priority.Value, source.Value,
            affectedEntityId, description,
            reference.IsEmpty ? null : reference.Domain,
            reference.IsEmpty ? null : reference.EntityId,
            sourceCorrelationId.IsEmpty ? null : sourceCorrelationId.Value));
        return incident;
    }

    public void Assign(IdentityId assigneeIdentityId, int escalationLevel = 1)
    {
        Guard.AgainstNull(assigneeIdentityId);

        if (Status.IsTerminal)
            throw new DomainException(IncidentErrors.NotActive, "Cannot assign a terminal incident.");

        Apply(new IncidentAssignedEvent(Id, assigneeIdentityId.Value, escalationLevel));
    }

    public void Escalate()
    {
        if (Status.IsTerminal)
            throw new DomainException(IncidentErrors.NotActive, "Cannot escalate a terminal incident.");

        if (!Severity.CanEscalate)
            throw new DomainException(IncidentErrors.CannotEscalate, "Incident is already at critical severity.");

        var previousSeverity = Severity;
        Apply(new IncidentEscalatedEvent(Id, previousSeverity.Value, previousSeverity.Escalate().Value, CurrentAssignment?.AssigneeIdentityId.Value));
    }

    public void UpdateDescription(string description)
    {
        Guard.AgainstEmpty(description);

        if (Status.IsTerminal)
            throw new DomainException(IncidentErrors.NotActive, "Cannot update a terminal incident.");

        Apply(new IncidentUpdatedEvent(Id, description));
    }

    public void Resolve()
    {
        if (Status.IsTerminal)
            throw new DomainException(IncidentErrors.AlreadyResolved, "Incident is already in a terminal state.");

        if (!Status.IsActive)
            throw new DomainException(IncidentErrors.NotActive, "Cannot resolve a non-active incident.");

        if (CurrentAssignment is null)
            throw new DomainException(IncidentErrors.NotAssigned, "Cannot resolve an unassigned incident.");

        Apply(new IncidentResolvedEvent(Id));
    }

    public void Close()
    {
        if (Status == IncidentStatus.Closed)
            throw new DomainException(IncidentErrors.AlreadyClosed, "Incident is already closed.");

        if (Status != IncidentStatus.Resolved)
            throw new DomainException(IncidentErrors.InvalidTransition, "Incident must be resolved before closing.");

        Apply(new IncidentClosedEvent(Id));
    }

    private void Apply(IncidentCreatedEvent e)
    {
        Id = e.IncidentId;
        IncidentId = new IncidentId(e.IncidentId);
        IncidentType = new IncidentType(e.IncidentType);
        Severity = new IncidentSeverity(e.Severity);
        Priority = new IncidentPriority(e.Priority);
        Source = new IncidentSource(e.Source);
        Status = IncidentStatus.Created;
        SLA = IncidentSLA.ForSeverity(Severity);
        AffectedEntityId = e.AffectedEntityId;
        Description = e.Description;
        Reference = e.ReferenceDomain is not null && e.ReferenceEntityId.HasValue
            ? new IncidentReference(e.ReferenceDomain, e.ReferenceEntityId.Value)
            : IncidentReference.None;
        SourceCorrelationId = e.SourceCorrelationId is not null
            ? new IncidentCorrelationId(e.SourceCorrelationId)
            : IncidentCorrelationId.Empty;
        EscalationLevel = 0;
        CreatedAt = e.OccurredAt;
        _timeline.Add(IncidentTimelineEntry.Record("created", $"Incident created: {e.Description}", e.OccurredAt));
        RaiseDomainEvent(e);
    }

    private void Apply(IncidentAssignedEvent e)
    {
        CurrentAssignment = IncidentAssignment.Create(new IdentityId(e.AssigneeIdentityId), e.EscalationLevel, e.OccurredAt);
        EscalationLevel = e.EscalationLevel;
        Status = IncidentStatus.Assigned;
        _timeline.Add(IncidentTimelineEntry.Record("assigned", $"Assigned to {e.AssigneeIdentityId} at level {e.EscalationLevel}", e.OccurredAt, e.AssigneeIdentityId));
        RaiseDomainEvent(e);
    }

    private void Apply(IncidentEscalatedEvent e)
    {
        Severity = new IncidentSeverity(e.NewSeverity);
        Priority = IncidentPriority.FromSeverity(Severity);
        SLA = IncidentSLA.ForSeverity(Severity);
        Status = IncidentStatus.Escalated;
        EscalationLevel++;
        _escalations.Add(IncidentEscalation.Create(
            EscalationLevel - 1, EscalationLevel,
            e.PreviousSeverity, e.NewSeverity, "Severity escalation", e.OccurredAt));
        _timeline.Add(IncidentTimelineEntry.Record("escalated", $"Escalated from {e.PreviousSeverity} to {e.NewSeverity}", e.OccurredAt));
        RaiseDomainEvent(e);
    }

    private void Apply(IncidentUpdatedEvent e)
    {
        Description = e.Description;
        _timeline.Add(IncidentTimelineEntry.Record("updated", "Description updated", e.OccurredAt));
        RaiseDomainEvent(e);
    }

    private void Apply(IncidentResolvedEvent e)
    {
        Status = IncidentStatus.Resolved;
        ResolvedAt = e.OccurredAt;
        _timeline.Add(IncidentTimelineEntry.Record("resolved", "Incident resolved", e.OccurredAt));
        RaiseDomainEvent(e);
    }

    private void Apply(IncidentClosedEvent e)
    {
        Status = IncidentStatus.Closed;
        _timeline.Add(IncidentTimelineEntry.Record("closed", "Incident closed", e.OccurredAt));
        RaiseDomainEvent(e);
    }
}

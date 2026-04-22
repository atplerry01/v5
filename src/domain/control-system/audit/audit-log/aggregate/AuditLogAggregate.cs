using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditLog;

public sealed class AuditLogAggregate : AggregateRoot
{
    public AuditLogId Id { get; private set; }
    public string ActorId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string? DecisionId { get; private set; }
    public AuditEntryClassification Classification { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditLogAggregate() { }

    public static AuditLogAggregate Record(
        AuditLogId id,
        string actorId,
        string action,
        string resource,
        AuditEntryClassification classification,
        DateTimeOffset occurredAt,
        string? decisionId = null)
    {
        Guard.Against(string.IsNullOrEmpty(actorId), "AuditLog entry requires an actorId.");
        Guard.Against(string.IsNullOrEmpty(action), "AuditLog entry requires an action.");
        Guard.Against(string.IsNullOrEmpty(resource), "AuditLog entry requires a resource.");

        var aggregate = new AuditLogAggregate();
        aggregate.RaiseDomainEvent(new AuditEntryRecordedEvent(
            id, actorId, action, resource, classification, occurredAt, decisionId));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        if (domainEvent is AuditEntryRecordedEvent e)
        {
            Id = e.Id;
            ActorId = e.ActorId;
            Action = e.Action;
            Resource = e.Resource;
            Classification = e.Classification;
            OccurredAt = e.OccurredAt;
            DecisionId = e.DecisionId;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "AuditLog entry must have a non-empty Id.");
    }
}

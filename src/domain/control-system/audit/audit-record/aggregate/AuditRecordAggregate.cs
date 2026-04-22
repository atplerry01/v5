using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditRecord;

public sealed class AuditRecordAggregate : AggregateRoot
{
    public AuditRecordId Id { get; private set; }
    public IReadOnlyList<string> AuditLogEntryIds { get; private set; } = [];
    public string Description { get; private set; } = string.Empty;
    public AuditRecordSeverity Severity { get; private set; }
    public AuditRecordStatus Status { get; private set; }
    public DateTimeOffset RaisedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    private AuditRecordAggregate() { }

    public static AuditRecordAggregate Raise(
        AuditRecordId id,
        IReadOnlyList<string> auditLogEntryIds,
        string description,
        AuditRecordSeverity severity,
        DateTimeOffset raisedAt)
    {
        Guard.Against(auditLogEntryIds is null || auditLogEntryIds.Count == 0,
            "AuditRecord must reference at least one audit log entry.");
        Guard.Against(string.IsNullOrEmpty(description), "AuditRecord description must not be empty.");

        var aggregate = new AuditRecordAggregate();
        aggregate.RaiseDomainEvent(new AuditRecordRaisedEvent(id, auditLogEntryIds!, description, severity, raisedAt));
        return aggregate;
    }

    public void Resolve(DateTimeOffset resolvedAt)
    {
        Guard.Against(Status == AuditRecordStatus.Resolved || Status == AuditRecordStatus.Closed,
            "AuditRecord is already resolved or closed.");

        RaiseDomainEvent(new AuditRecordResolvedEvent(Id, resolvedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AuditRecordRaisedEvent e:
                Id = e.Id;
                AuditLogEntryIds = e.AuditLogEntryIds;
                Description = e.Description;
                Severity = e.Severity;
                Status = AuditRecordStatus.Open;
                RaisedAt = e.RaisedAt;
                break;
            case AuditRecordResolvedEvent e:
                Status = AuditRecordStatus.Resolved;
                ResolvedAt = e.ResolvedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "AuditRecord must have a non-empty Id.");
        Guard.Against(AuditLogEntryIds.Count == 0, "AuditRecord must reference at least one audit log entry.");
    }
}

using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditTrace;

public sealed class AuditTraceAggregate : AggregateRoot
{
    public AuditTraceId Id { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public IReadOnlyList<string> LinkedAuditEventIds { get; private set; } = new List<string>();
    public TraceStatus Status { get; private set; }

    private AuditTraceAggregate() { }

    public static AuditTraceAggregate Open(
        AuditTraceId id,
        string correlationId,
        DateTimeOffset openedAt)
    {
        Guard.Against(string.IsNullOrEmpty(correlationId), AuditTraceErrors.CorrelationIdMustNotBeEmpty().Message);

        var aggregate = new AuditTraceAggregate();
        aggregate.RaiseDomainEvent(new AuditTraceOpenedEvent(id, correlationId, openedAt));
        return aggregate;
    }

    public void LinkEvent(string auditEventId)
    {
        Guard.Against(string.IsNullOrEmpty(auditEventId), AuditTraceErrors.AuditEventIdMustNotBeEmpty().Message);
        Guard.Against(Status == TraceStatus.Closed, AuditTraceErrors.CannotLinkEventToClosedTrace().Message);

        RaiseDomainEvent(new AuditTraceEventLinkedEvent(Id, auditEventId));
    }

    public void Close(DateTimeOffset closedAt)
    {
        Guard.Against(Status == TraceStatus.Closed, AuditTraceErrors.TraceAlreadyClosed().Message);

        RaiseDomainEvent(new AuditTraceClosedEvent(Id, closedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AuditTraceOpenedEvent e:
                Id = e.Id;
                CorrelationId = e.CorrelationId;
                OpenedAt = e.OpenedAt;
                Status = TraceStatus.Open;
                break;
            case AuditTraceEventLinkedEvent e:
                LinkedAuditEventIds = new List<string>(LinkedAuditEventIds) { e.AuditEventId };
                break;
            case AuditTraceClosedEvent e:
                ClosedAt = e.ClosedAt;
                Status = TraceStatus.Closed;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "AuditTrace must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(CorrelationId), "AuditTrace must have a non-empty CorrelationId.");
    }
}

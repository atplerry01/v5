using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditEvent;

public sealed class AuditEventAggregate : AggregateRoot
{
    public AuditEventId Id { get; private set; }
    public string ActorId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public AuditEventKind Kind { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public bool IsSealed { get; private set; }
    public string? IntegrityHash { get; private set; }

    private AuditEventAggregate() { }

    public static AuditEventAggregate Capture(
        AuditEventId id,
        string actorId,
        string action,
        AuditEventKind kind,
        string correlationId,
        DateTimeOffset occurredAt)
    {
        Guard.Against(string.IsNullOrEmpty(actorId), AuditEventErrors.ActorIdMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(action), AuditEventErrors.ActionMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(correlationId), AuditEventErrors.CorrelationIdMustNotBeEmpty().Message);

        var aggregate = new AuditEventAggregate();
        aggregate.RaiseDomainEvent(new AuditEventCapturedEvent(id, actorId, action, kind, correlationId, occurredAt));
        return aggregate;
    }

    public void Seal(string integrityHash)
    {
        Guard.Against(IsSealed, AuditEventErrors.AuditEventAlreadySealed().Message);

        RaiseDomainEvent(new AuditEventSealedEvent(Id, integrityHash));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AuditEventCapturedEvent e:
                Id = e.Id;
                ActorId = e.ActorId;
                Action = e.Action;
                Kind = e.Kind;
                CorrelationId = e.CorrelationId;
                OccurredAt = e.OccurredAt;
                break;
            case AuditEventSealedEvent e:
                IsSealed = true;
                IntegrityHash = e.IntegrityHash;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "AuditEvent must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(ActorId), "AuditEvent must have a non-empty ActorId.");
        Guard.Against(string.IsNullOrEmpty(CorrelationId), "AuditEvent must have a non-empty CorrelationId.");
    }
}

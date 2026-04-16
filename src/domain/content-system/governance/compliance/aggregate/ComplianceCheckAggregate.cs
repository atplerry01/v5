using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Compliance;

public sealed class ComplianceCheckAggregate : AggregateRoot
{
    private static readonly ComplianceSpecification Spec = new();

    public ComplianceCheckId CheckId { get; private set; }
    public string SubjectRef { get; private set; } = string.Empty;
    public ComplianceRuleRef Rule { get; private set; } = default!;
    public ComplianceCheckStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public Timestamp InitiatedAt { get; private set; }

    private ComplianceCheckAggregate() { }

    public static ComplianceCheckAggregate Initiate(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        ComplianceCheckId id, string subjectRef, ComplianceRuleRef rule, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(subjectRef)) throw ComplianceErrors.InvalidSubjectRef();
        var agg = new ComplianceCheckAggregate();
        agg.RaiseDomainEvent(new ComplianceCheckInitiatedEvent(eventId, aggregateId, correlationId, causationId, id, subjectRef, rule.Value, at));
        return agg;
    }

    public void Pass(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsureInitiated(Status);
        RaiseDomainEvent(new ComplianceCheckPassedEvent(eventId, aggregateId, correlationId, causationId, CheckId, at));
    }

    public void Fail(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string reason, Timestamp at)
    {
        Spec.EnsureInitiated(Status);
        RaiseDomainEvent(new ComplianceCheckFailedEvent(eventId, aggregateId, correlationId, causationId, CheckId, reason ?? string.Empty, at));
    }

    public void Expire(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsureInitiated(Status);
        RaiseDomainEvent(new ComplianceCheckExpiredEvent(eventId, aggregateId, correlationId, causationId, CheckId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ComplianceCheckInitiatedEvent e:
                CheckId = e.CheckId;
                SubjectRef = e.SubjectRef;
                Rule = ComplianceRuleRef.Create(e.RuleRef);
                Status = ComplianceCheckStatus.Initiated;
                InitiatedAt = e.InitiatedAt;
                break;
            case ComplianceCheckPassedEvent: Status = ComplianceCheckStatus.Passed; break;
            case ComplianceCheckFailedEvent e:
                Status = ComplianceCheckStatus.Failed;
                FailureReason = e.Reason;
                break;
            case ComplianceCheckExpiredEvent: Status = ComplianceCheckStatus.Expired; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(SubjectRef))
            throw ComplianceErrors.SubjectMissing();
    }
}

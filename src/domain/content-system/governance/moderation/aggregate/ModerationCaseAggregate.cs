using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public sealed class ModerationCaseAggregate : AggregateRoot
{
    private static readonly ModerationSpecification Spec = new();
    private readonly Dictionary<Guid, EvidenceRecord> _evidence = new();

    public ModerationCaseId CaseId { get; private set; }
    public string TargetRef { get; private set; } = string.Empty;
    public ModerationCaseStatus Status { get; private set; }
    public ModerationDecision Decision { get; private set; }
    public Timestamp OpenedAt { get; private set; }
    public IReadOnlyCollection<EvidenceRecord> Evidence => _evidence.Values;

    private ModerationCaseAggregate() { }

    public static ModerationCaseAggregate Open(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        ModerationCaseId id, string targetRef, string reporterRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(targetRef)) throw ModerationErrors.InvalidTargetRef();
        if (string.IsNullOrWhiteSpace(reporterRef)) throw ModerationErrors.InvalidReporter();
        var agg = new ModerationCaseAggregate();
        agg.RaiseDomainEvent(new ModerationCaseOpenedEvent(eventId, aggregateId, correlationId, causationId, id, targetRef, reporterRef, at));
        return agg;
    }

    public void AttachEvidence(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, EvidenceRecord evidence, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        if (_evidence.ContainsKey(evidence.EvidenceId)) throw ModerationErrors.InvalidEvidence();
        RaiseDomainEvent(new ModerationEvidenceAttachedEvent(
            eventId, aggregateId, correlationId, causationId, CaseId, evidence.EvidenceId, evidence.ReporterRef, evidence.Description, at));
    }

    public void IssueDecision(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, ModerationDecision decision, string deciderRef, string rationale, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        Spec.EnsureValidDecision(decision);
        if (string.IsNullOrWhiteSpace(deciderRef)) throw ModerationErrors.InvalidReporter();
        RaiseDomainEvent(new ModerationDecisionIssuedEvent(
            eventId, aggregateId, correlationId, causationId, CaseId, decision, deciderRef, rationale ?? string.Empty, at));
    }

    public void Appeal(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string appellantRef, string grounds, Timestamp at)
    {
        Spec.EnsureDecided(Status);
        if (string.IsNullOrWhiteSpace(appellantRef)) throw ModerationErrors.InvalidReporter();
        RaiseDomainEvent(new ModerationCaseAppealedEvent(eventId, aggregateId, correlationId, causationId, CaseId, appellantRef, grounds ?? string.Empty, at));
    }

    public void Close(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == ModerationCaseStatus.Closed) throw ModerationErrors.AlreadyClosed();
        RaiseDomainEvent(new ModerationCaseClosedEvent(eventId, aggregateId, correlationId, causationId, CaseId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ModerationCaseOpenedEvent e:
                CaseId = e.CaseId;
                TargetRef = e.TargetRef;
                Status = ModerationCaseStatus.Opened;
                OpenedAt = e.OpenedAt;
                break;
            case ModerationEvidenceAttachedEvent e:
                Status = ModerationCaseStatus.UnderReview;
                _evidence[e.EvidenceId] = EvidenceRecord.Attach(e.EvidenceId, e.ReporterRef, e.Description, e.AttachedAt);
                break;
            case ModerationDecisionIssuedEvent e:
                Status = ModerationCaseStatus.Decided;
                Decision = e.Decision;
                break;
            case ModerationCaseAppealedEvent: Status = ModerationCaseStatus.Appealed; break;
            case ModerationCaseClosedEvent: Status = ModerationCaseStatus.Closed; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(TargetRef))
            throw ModerationErrors.TargetMissing();
    }
}

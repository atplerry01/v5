using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Compliance.Audit;

public sealed class AuditRecordAggregate : AggregateRoot
{
    public AuditRecordId AuditRecordId { get; private set; }
    public SourceDomain SourceDomain { get; private set; }
    public SourceAggregateId SourceAggregateId { get; private set; }
    public SourceEventId SourceEventId { get; private set; }
    public AuditType AuditType { get; private set; }
    public AuditRecordStatus Status { get; private set; }
    public EvidenceSummary EvidenceSummary { get; private set; }
    public Timestamp RecordedAt { get; private set; }
    public Timestamp FinalizedAt { get; private set; }

    private AuditRecordAggregate() { }

    public static AuditRecordAggregate CreateRecord(
        AuditRecordId auditRecordId,
        SourceDomain sourceDomain,
        SourceAggregateId sourceAggregateId,
        SourceEventId sourceEventId,
        AuditType auditType,
        EvidenceSummary evidenceSummary,
        Timestamp recordedAt)
    {
        var aggregate = new AuditRecordAggregate();
        aggregate.RaiseDomainEvent(new AuditRecordCreatedEvent(
            auditRecordId, sourceDomain, sourceAggregateId, sourceEventId,
            auditType, evidenceSummary, recordedAt));
        return aggregate;
    }

    public void FinalizeRecord(Timestamp finalizedAt)
    {
        Guard.Against(Status == AuditRecordStatus.Finalized, AuditErrors.AlreadyFinalized);
        Guard.Against(Status != AuditRecordStatus.Draft, AuditErrors.InvalidStateTransition);

        RaiseDomainEvent(new AuditRecordFinalizedEvent(AuditRecordId, finalizedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AuditRecordCreatedEvent e:
                AuditRecordId = e.AuditRecordId;
                SourceDomain = e.SourceDomain;
                SourceAggregateId = e.SourceAggregateId;
                SourceEventId = e.SourceEventId;
                AuditType = e.AuditType;
                EvidenceSummary = e.EvidenceSummary;
                Status = AuditRecordStatus.Draft;
                RecordedAt = e.RecordedAt;
                break;

            case AuditRecordFinalizedEvent e:
                Status = AuditRecordStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
    }
}
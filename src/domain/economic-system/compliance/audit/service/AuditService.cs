using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Compliance.Audit;

public sealed class AuditService
{
    private readonly AuditRecordSpecification _specification;

    public AuditService()
    {
        _specification = new AuditRecordSpecification();
    }

    public AuditRecordAggregate RecordEvidence(
        AuditRecordId auditRecordId,
        SourceDomain sourceDomain,
        SourceAggregateId sourceAggregateId,
        SourceEventId sourceEventId,
        AuditType auditType,
        DocumentRef evidenceSummary,
        Timestamp recordedAt)
    {
        return AuditRecordAggregate.CreateRecord(
            auditRecordId,
            sourceDomain,
            sourceAggregateId,
            sourceEventId,
            auditType,
            evidenceSummary,
            recordedAt);
    }

    public void Finalize(AuditRecordAggregate aggregate, Timestamp finalizedAt)
    {
        Guard.Against(!_specification.CanFinalize(aggregate.Status), AuditErrors.InvalidStateTransition);
        aggregate.FinalizeRecord(finalizedAt);
    }
}

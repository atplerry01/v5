namespace Whycespace.Domain.EconomicSystem.Compliance.Audit;

public sealed class AuditRecordSpecification
{
    public bool CanFinalize(AuditRecordStatus status)
    {
        return status == AuditRecordStatus.Draft;
    }

    public bool IsFinalized(AuditRecordStatus status)
    {
        return status == AuditRecordStatus.Finalized;
    }
}
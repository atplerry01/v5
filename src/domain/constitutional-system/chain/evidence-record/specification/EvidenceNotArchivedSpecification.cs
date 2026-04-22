namespace Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;

public static class EvidenceNotArchivedSpecification
{
    public static bool IsSatisfiedBy(EvidenceRecordStatus status) =>
        status != EvidenceRecordStatus.Archived;
}

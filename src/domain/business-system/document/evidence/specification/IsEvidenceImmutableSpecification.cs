namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed class IsEvidenceImmutableSpecification
{
    public bool IsSatisfiedBy(EvidenceStatus status)
    {
        return status == EvidenceStatus.Captured
            || status == EvidenceStatus.Verified
            || status == EvidenceStatus.Archived;
    }
}

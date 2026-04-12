namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed class CanArchiveEvidenceSpecification
{
    public bool IsSatisfiedBy(EvidenceStatus status)
    {
        return status == EvidenceStatus.Verified;
    }
}

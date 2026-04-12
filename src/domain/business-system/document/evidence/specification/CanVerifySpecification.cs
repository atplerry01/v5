namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed class CanVerifySpecification
{
    public bool IsSatisfiedBy(EvidenceStatus status)
    {
        return status == EvidenceStatus.Captured;
    }
}

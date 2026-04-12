namespace Whycespace.Domain.BusinessSystem.Document.SignatureRecord;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(SignatureRecordStatus status)
    {
        return status == SignatureRecordStatus.Verified;
    }
}

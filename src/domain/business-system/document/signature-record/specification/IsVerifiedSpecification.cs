namespace Whycespace.Domain.BusinessSystem.Document.SignatureRecord;

public sealed class IsVerifiedSpecification
{
    public bool IsSatisfiedBy(SignatureRecordStatus status)
    {
        return status == SignatureRecordStatus.Verified || status == SignatureRecordStatus.Archived;
    }
}

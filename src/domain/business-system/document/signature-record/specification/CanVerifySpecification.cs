namespace Whycespace.Domain.BusinessSystem.Document.SignatureRecord;

public sealed class CanVerifySpecification
{
    public bool IsSatisfiedBy(SignatureRecordStatus status)
    {
        return status == SignatureRecordStatus.Captured;
    }
}

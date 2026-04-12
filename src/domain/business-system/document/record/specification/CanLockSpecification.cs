namespace Whycespace.Domain.BusinessSystem.Document.Record;

public sealed class CanLockSpecification
{
    public bool IsSatisfiedBy(RecordStatus status)
    {
        return status == RecordStatus.Active;
    }
}

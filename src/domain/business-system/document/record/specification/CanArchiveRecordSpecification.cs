namespace Whycespace.Domain.BusinessSystem.Document.Record;

public sealed class CanArchiveRecordSpecification
{
    public bool IsSatisfiedBy(RecordStatus status)
    {
        return status == RecordStatus.Locked;
    }
}

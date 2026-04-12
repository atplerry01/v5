namespace Whycespace.Domain.BusinessSystem.Document.Record;

public sealed class IsRecordModifiableSpecification
{
    public bool IsSatisfiedBy(RecordStatus status)
    {
        return status == RecordStatus.Active;
    }
}

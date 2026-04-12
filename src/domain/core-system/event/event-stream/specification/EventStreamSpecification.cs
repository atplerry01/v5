namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public sealed class CanSealSpecification
{
    public bool IsSatisfiedBy(EventStreamStatus status)
    {
        return status == EventStreamStatus.Open;
    }
}

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(EventStreamStatus status)
    {
        return status == EventStreamStatus.Sealed;
    }
}

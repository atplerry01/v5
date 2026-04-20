namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public static class SegmentErrors
{
    public static SegmentDomainException MissingId()
        => new("SegmentId is required and must not be empty.");

    public static SegmentDomainException InvalidStateTransition(SegmentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static SegmentDomainException ArchivedImmutable(SegmentId id)
        => new($"Segment '{id.Value}' is archived and cannot be mutated.");
}

public sealed class SegmentDomainException : Exception
{
    public SegmentDomainException(string message) : base(message) { }
}

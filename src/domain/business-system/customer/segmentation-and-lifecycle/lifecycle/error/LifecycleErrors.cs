namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public static class LifecycleErrors
{
    public static LifecycleDomainException MissingId()
        => new("LifecycleId is required and must not be empty.");

    public static LifecycleDomainException MissingCustomerRef()
        => new("Lifecycle must reference a customer.");

    public static LifecycleDomainException InvalidStageTransition(LifecycleStage from, LifecycleStage to)
        => new($"Cannot transition lifecycle from '{from}' to '{to}'.");

    public static LifecycleDomainException ClosedImmutable(LifecycleId id)
        => new($"Lifecycle '{id.Value}' is closed and cannot be mutated.");
}

public sealed class LifecycleDomainException : Exception
{
    public LifecycleDomainException(string message) : base(message) { }
}

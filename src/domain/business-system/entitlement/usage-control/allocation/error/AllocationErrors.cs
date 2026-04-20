namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public static class AllocationErrors
{
    public static AllocationDomainException MissingId()
        => new("AllocationId is required and must not be empty.");

    public static AllocationDomainException MissingResourceId()
        => new("ResourceId is required and must not be empty.");

    public static AllocationDomainException InvalidStateTransition(AllocationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static AllocationDomainException CapacityExceeded(int requested, int available)
        => new($"Cannot allocate {requested} units. Only {available} available.");

    public static AllocationDomainException AlreadyAllocated(AllocationId id)
        => new($"Allocation '{id.Value}' has already been allocated.");

    public static AllocationDomainException AlreadyReleased(AllocationId id)
        => new($"Allocation '{id.Value}' has already been released.");
}

public sealed class AllocationDomainException : Exception
{
    public AllocationDomainException(string message) : base(message) { }
}
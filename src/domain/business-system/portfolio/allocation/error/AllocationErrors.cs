namespace Whycespace.Domain.BusinessSystem.Portfolio.Allocation;

public static class AllocationErrors
{
    public static AllocationDomainException MissingId()
        => new("AllocationId is required and must not be empty.");

    public static AllocationDomainException InvalidStateTransition(AllocationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static AllocationDomainException TargetReferenceRequired()
        => new("Allocation must reference a target.");

    public static AllocationDomainException PortfolioReferenceRequired()
        => new("Allocation must reference a portfolio.");

    public static AllocationDomainException WeightOutOfBounds()
        => new("Allocation weight must be greater than zero and at most 1.");
}

public sealed class AllocationDomainException : Exception
{
    public AllocationDomainException(string message) : base(message) { }
}

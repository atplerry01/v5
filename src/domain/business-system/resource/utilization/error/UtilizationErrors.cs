namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public static class UtilizationErrors
{
    public static UtilizationDomainException MissingId()
        => new("UtilizationId is required and must not be empty.");

    public static UtilizationDomainException InvalidStateTransition(UtilizationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static UtilizationDomainException ResourceReferenceRequired()
        => new("Utilization must reference a resource.");

    public static UtilizationDomainException UsageMustNotBeNegative()
        => new("Usage amount must not be negative.");

    public static UtilizationDomainException ExceedsCapacityConstraint(decimal usage, int capacityLimit)
        => new($"Cumulative usage '{usage}' exceeds capacity limit '{capacityLimit}'.");

    public static UtilizationDomainException CapacityLimitRequired()
        => new("Capacity limit must be greater than zero.");
}

public sealed class UtilizationDomainException : Exception
{
    public UtilizationDomainException(string message) : base(message) { }
}

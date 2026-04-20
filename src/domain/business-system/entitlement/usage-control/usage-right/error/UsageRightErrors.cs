namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public static class UsageRightErrors
{
    public static UsageRightDomainException MissingId()
        => new("UsageRightId is required and must not be empty.");

    public static UsageRightDomainException MissingSubjectId()
        => new("UsageRightSubjectId is required and must not be empty.");

    public static UsageRightDomainException MissingReferenceId()
        => new("UsageRightReferenceId is required and must not be empty.");

    public static UsageRightDomainException InvalidTotalUnits()
        => new("TotalUnits must be greater than zero.");

    public static UsageRightDomainException InvalidStateTransition(UsageRightStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static UsageRightDomainException UsageExceedsAvailable(int requested, int remaining)
        => new($"Cannot use {requested} units. Only {remaining} remaining.");

    public static UsageRightDomainException UsageRemaining(int remaining)
        => new($"Cannot consume usage right with {remaining} units remaining.");

    public static UsageRightDomainException AlreadyConsumed(UsageRightId id)
        => new($"UsageRight '{id.Value}' has already been fully consumed.");
}

public sealed class UsageRightDomainException : Exception
{
    public UsageRightDomainException(string message) : base(message) { }
}

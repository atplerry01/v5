namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public static class CapacityErrors
{
    public static CapacityDomainException MissingId()
        => new("CapacityId is required and must not be empty.");

    public static CapacityDomainException MissingLimit()
        => new("Capacity must define a limit.");

    public static CapacityDomainException AlreadyActive(CapacityId id)
        => new($"Capacity '{id.Value}' is already active.");

    public static CapacityDomainException AlreadySuspended(CapacityId id)
        => new($"Capacity '{id.Value}' is already suspended.");

    public static CapacityDomainException InvalidStateTransition(CapacityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class CapacityDomainException : Exception
{
    public CapacityDomainException(string message) : base(message) { }
}

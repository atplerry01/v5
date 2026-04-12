namespace Whycespace.Domain.BusinessSystem.Integration.Synchronization;

public static class SynchronizationErrors
{
    public static SynchronizationDomainException MissingId()
        => new("SynchronizationId is required and must not be empty.");

    public static SynchronizationDomainException MissingPolicyId()
        => new("SyncPolicyId is required and must not be empty.");

    public static SynchronizationDomainException AlreadyPending(SynchronizationId id)
        => new($"Synchronization '{id.Value}' is already pending.");

    public static SynchronizationDomainException AlreadyConfirmed(SynchronizationId id)
        => new($"Synchronization '{id.Value}' has already been confirmed.");

    public static SynchronizationDomainException InvalidStateTransition(SynchronizationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SynchronizationDomainException : Exception
{
    public SynchronizationDomainException(string message) : base(message) { }
}

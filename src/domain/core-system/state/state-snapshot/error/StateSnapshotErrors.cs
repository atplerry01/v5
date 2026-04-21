using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public static class StateSnapshotErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("StateSnapshotId requires a non-empty Guid.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("StateSnapshot requires a valid SnapshotDescriptor.");

    public static DomainException InvalidStateTransition(SnapshotStatus status, string action)
        => new DomainInvariantViolationException($"Cannot {action} a snapshot in {status} status.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("StateSnapshot has already been initialized.");
}

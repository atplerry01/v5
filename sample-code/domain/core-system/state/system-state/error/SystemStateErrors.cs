using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.State.SystemState;

public sealed class SystemStateNotActiveException : DomainException
{
    public SystemStateNotActiveException(string currentStatus)
        : base("SYSTEM_STATE_NOT_ACTIVE", $"System state must be Active to perform this operation. Current: {currentStatus}.") { }
}

public sealed class StateValidationFailedException : DomainException
{
    public StateValidationFailedException(string systemName, string details)
        : base("STATE_VALIDATION_FAILED", $"State validation failed for system '{systemName}': {details}") { }
}

public sealed class NoSnapshotAvailableException : DomainException
{
    public NoSnapshotAvailableException()
        : base("NO_SNAPSHOT_AVAILABLE", "Cannot declare authoritative state without a captured snapshot.") { }
}

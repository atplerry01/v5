using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public static class StreamSessionErrors
{
    public static DomainException SessionAlreadyTerminal()
        => new("Stream session is already closed, failed, or expired.");

    public static DomainException CannotActivateUnlessOpened()
        => new("Stream session must be in opened state to be activated.");

    public static DomainException CannotSuspendUnlessActive()
        => new("Only an active stream session can be suspended.");

    public static DomainException CannotResumeUnlessSuspended()
        => new("Only a suspended stream session can be resumed.");

    public static DomainException OpenedAfterExpiry()
        => new("Stream session window has already expired at open time.");

    public static DomainInvariantViolationException OrphanedSession()
        => new("Stream session must reference a parent stream.");
}

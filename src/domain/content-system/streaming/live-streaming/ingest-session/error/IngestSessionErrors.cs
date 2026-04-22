using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public static class IngestSessionErrors
{
    public static DomainException SessionAlreadyTerminal()
        => new("Ingest session is already in a terminal state (Ended or Failed).");

    public static DomainException CannotStartUnlessAuthenticated()
        => new("Ingest streaming can only start from Authenticated status.");

    public static DomainException CannotStallUnlessStreaming()
        => new("Ingest session can only stall while Streaming.");

    public static DomainException CannotResumeUnlessStalled()
        => new("Ingest session can only resume from Stalled status.");

    public static DomainException EmptyFailureReason()
        => new("Ingest session failure reason cannot be empty.");

    public static DomainInvariantViolationException MissingSessionId()
        => new("IngestSession requires a valid SessionId.");

    public static DomainInvariantViolationException MissingBroadcastRef()
        => new("IngestSession requires a valid BroadcastRef.");
}

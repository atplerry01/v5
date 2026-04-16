using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public static class StreamSessionErrors
{
    public static DomainException InvalidEndpoint() => new("Stream endpoint must be an absolute URI.");
    public static DomainException InvalidAssetRef() => new("Stream asset reference must be non-empty.");
    public static DomainException InvalidViewer() => new("Viewer reference must be non-empty.");
    public static DomainException ViewerAlreadyLeft() => new("Viewer has already left the stream.");
    public static DomainException ViewerNotInSession() => new("Viewer is not in this stream.");
    public static DomainException SessionNotOpen() => new("Stream session is not open.");
    public static DomainException AlreadyTerminal() => new("Stream session already terminated.");
    public static DomainInvariantViolationException AssetMissing() =>
        new("Invariant violated: stream session must reference an asset.");
}

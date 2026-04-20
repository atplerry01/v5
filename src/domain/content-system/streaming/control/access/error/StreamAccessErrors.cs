using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.Control.Access;

public static class StreamAccessErrors
{
    public static DomainException AccessAlreadyRevoked()
        => new("Stream access is already revoked.");

    public static DomainException AccessAlreadyExpired()
        => new("Stream access is already expired.");

    public static DomainException AccessAlreadyRestricted()
        => new("Stream access is already restricted.");

    public static DomainException AccessNotRestricted()
        => new("Stream access is not restricted.");

    public static DomainException CannotMutateTerminalAccess()
        => new("Cannot mutate revoked or expired access.");

    public static DomainException InvalidReason()
        => new("Reason cannot be empty.");
}

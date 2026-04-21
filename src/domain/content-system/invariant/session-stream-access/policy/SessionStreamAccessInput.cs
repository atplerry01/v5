namespace Whycespace.Domain.ContentSystem.Invariant.SessionStreamAccess;

/// <summary>
/// Minimum fact-set for SessionStreamAccessPolicy. Assembled by the handler
/// from the StreamAccess projection (or aggregate) for the (subject, stream)
/// pair at the activation instant.
/// </summary>
public readonly record struct SessionStreamAccessInput(
    Guid StreamId,
    Guid SubjectId,
    bool AccessGrantExists,
    bool AccessGrantRevoked,
    bool AccessGrantExpired,
    bool AccessGrantRestricted);

namespace Whycespace.Domain.TrustSystem.Access.Session;

public static class SessionErrors
{
    public static DomainException NotFound(Guid sessionId)
        => new("SESSION.NOT_FOUND", $"Session '{sessionId}' not found.");

    public static DomainException AlreadyExpired(Guid sessionId)
        => new("SESSION.ALREADY_EXPIRED", $"Session '{sessionId}' has already expired.");

    public static DomainException InvalidDevice(Guid deviceId)
        => new("SESSION.INVALID_DEVICE", $"Device '{deviceId}' is not valid for session creation.");
}

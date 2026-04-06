namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed class SessionService
{
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(24);

    public bool IsExpired(SessionAggregate session, DateTimeOffset currentTime)
    {
        if (session.Status != SessionStatus.Active)
            return true;

        return currentTime - session.LastActivityAt > _sessionTimeout;
    }

    public SessionAggregate StartSession(
        Guid id,
        Guid identityId,
        Guid deviceId,
        string ipAddress,
        DateTimeOffset timestamp)
    {
        return SessionAggregate.Start(id, identityId, deviceId, ipAddress, timestamp);
    }
}

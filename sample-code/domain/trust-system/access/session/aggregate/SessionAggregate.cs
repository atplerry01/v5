namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed class SessionAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public Guid DeviceId { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Active;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public DateTimeOffset LastActivityAt { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;

    private static readonly Dictionary<SessionStatus, HashSet<SessionStatus>> ValidTransitions = new()
    {
        [SessionStatus.Active] = [SessionStatus.Expired, SessionStatus.Revoked],
        [SessionStatus.Expired] = [],
        [SessionStatus.Revoked] = []
    };

    private SessionAggregate() { }

    public static SessionAggregate Start(
        Guid id,
        Guid identityId,
        Guid deviceId,
        string ipAddress,
        DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstDefault(identityId);
        Guard.AgainstDefault(deviceId);

        var aggregate = new SessionAggregate
        {
            Id = id,
            IdentityId = identityId,
            DeviceId = deviceId,
            IpAddress = ipAddress,
            Status = SessionStatus.Active,
            StartedAt = timestamp,
            LastActivityAt = timestamp
        };

        aggregate.RaiseDomainEvent(new SessionStartedEvent(id, identityId, deviceId, ipAddress));
        return aggregate;
    }

    public void RecordActivity(DateTimeOffset timestamp)
    {
        EnsureInvariant(Status == SessionStatus.Active, "SESSION_MUST_BE_ACTIVE", "Cannot record activity on inactive session.");

        LastActivityAt = timestamp;
        RaiseDomainEvent(new SessionActivityRecordedEvent(Id, IdentityId, timestamp));
    }

    public void Expire(DateTimeOffset timestamp)
    {
        EnsureValidTransition(Status, SessionStatus.Expired, (from, to) => ValidTransitions[from].Contains(to));

        Status = SessionStatus.Expired;
        EndedAt = timestamp;
        RaiseDomainEvent(new SessionExpiredEvent(Id, IdentityId));
    }

    public void Revoke(string reason, DateTimeOffset timestamp)
    {
        EnsureValidTransition(Status, SessionStatus.Revoked, (from, to) => ValidTransitions[from].Contains(to));

        Status = SessionStatus.Revoked;
        EndedAt = timestamp;
        RaiseDomainEvent(new SessionRevokedEvent(Id, IdentityId, reason));
    }
}

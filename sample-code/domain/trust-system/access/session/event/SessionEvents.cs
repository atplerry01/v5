namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed record SessionStartedEvent(
    Guid SessionId,
    Guid IdentityId,
    Guid DeviceId,
    string IpAddress) : DomainEvent;

public sealed record SessionActivityRecordedEvent(
    Guid SessionId,
    Guid IdentityId,
    DateTimeOffset ActivityAt) : DomainEvent;

public sealed record SessionExpiredEvent(
    Guid SessionId,
    Guid IdentityId) : DomainEvent;

public sealed record SessionRevokedEvent(
    Guid SessionId,
    Guid IdentityId,
    string Reason) : DomainEvent;

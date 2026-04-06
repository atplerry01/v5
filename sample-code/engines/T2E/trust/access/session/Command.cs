namespace Whycespace.Engines.T2E.Trust.Access.Session;

public record SessionCommand(string Action, string EntityId, object Payload);
public sealed record StartSessionCommand(string IdentityId, string DeviceId, string ExpiresAt) : SessionCommand("Start", IdentityId, null!);
public sealed record RefreshSessionCommand(string SessionId, string NewExpiresAt) : SessionCommand("Refresh", SessionId, null!);
public sealed record RevokeSessionCommand(string SessionId) : SessionCommand("Revoke", SessionId, null!);
public sealed record ExpireSessionCommand(string SessionId) : SessionCommand("Expire", SessionId, null!);

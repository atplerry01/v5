namespace Whycespace.Engines.T2E.Trust.Access.Session;

public record SessionResult(bool Success, string Message);
public sealed record SessionDto(string SessionId, string IdentityId, string DeviceId, string Status);

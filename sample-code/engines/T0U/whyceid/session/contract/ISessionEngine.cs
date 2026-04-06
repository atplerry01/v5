namespace Whycespace.Engines.T0U.WhyceId.Session;

public interface ISessionEngine
{
    SessionDecisionResult Evaluate(SessionDecisionCommand command);
}

public sealed record SessionDecisionCommand(
    string IdentityId,
    string DeviceId,
    int ActiveSessionCount,
    int MaxConcurrentSessions);

public sealed record SessionDecisionResult(
    bool CanCreate,
    string? Reason = null)
{
    public static SessionDecisionResult Allow() => new(true);
    public static SessionDecisionResult Deny(string reason) => new(false, reason);
}

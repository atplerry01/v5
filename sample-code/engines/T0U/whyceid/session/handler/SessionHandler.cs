namespace Whycespace.Engines.T0U.WhyceId.Session;

public sealed class SessionHandler : ISessionEngine
{
    public SessionDecisionResult Evaluate(SessionDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return SessionDecisionResult.Deny("IdentityId is required.");

        if (command.ActiveSessionCount >= command.MaxConcurrentSessions)
            return SessionDecisionResult.Deny($"Maximum concurrent sessions ({command.MaxConcurrentSessions}) reached.");

        return SessionDecisionResult.Allow();
    }
}

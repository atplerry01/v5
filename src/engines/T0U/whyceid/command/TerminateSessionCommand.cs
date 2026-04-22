namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record TerminateSessionCommand(
    string SessionId,
    string IdentityId);

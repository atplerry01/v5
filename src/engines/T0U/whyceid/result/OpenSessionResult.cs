namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record OpenSessionResult(
    string SessionId,
    bool IsOpened,
    string? FailureReason);

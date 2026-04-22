namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record TerminateSessionResult(
    bool IsTerminated,
    string? FailureReason);

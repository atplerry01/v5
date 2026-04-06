namespace Whyce.Engines.T0U.WhyceId.Result;

public sealed record ValidateSessionResult(bool IsValid, string? FailureReason);

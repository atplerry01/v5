namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record RevokeConsentResult(
    bool IsRevoked,
    string? FailureReason);

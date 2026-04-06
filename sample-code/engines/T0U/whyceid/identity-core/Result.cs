namespace Whycespace.Engines.T0U.WhyceId;

public sealed record IdentityResult(bool IsVerified, string SubjectId);

public sealed record AuthorizationResult(bool IsAuthorized, string Reason);

public sealed record TrustScoreResult(string SubjectId, decimal Score);

/// <summary>
/// Generic validation result returned by T0U identity validation engines.
/// T2E checks IsValid before proceeding with execution.
/// </summary>
public sealed record IdentityValidationResult(bool IsValid, string? Reason = null)
{
    public static IdentityValidationResult Valid() => new(true);
    public static IdentityValidationResult Invalid(string reason) => new(false, reason);
}

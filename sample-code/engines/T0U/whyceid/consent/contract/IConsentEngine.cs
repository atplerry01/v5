namespace Whycespace.Engines.T0U.WhyceId.Consent;

public interface IConsentEngine
{
    ConsentDecisionResult Evaluate(ConsentDecisionCommand command);
}

public sealed record ConsentDecisionCommand(
    string IdentityId,
    string ConsentType,
    string Scope,
    bool HasExistingConsent);

public sealed record ConsentDecisionResult(
    bool IsPermitted,
    string? Reason = null)
{
    public static ConsentDecisionResult Permit() => new(true);
    public static ConsentDecisionResult Deny(string reason) => new(false, reason);
}

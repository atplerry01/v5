namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public static class VerificationErrors
{
    public static InvalidOperationException MissingId() =>
        new("Verification identifier must not be empty.");

    public static InvalidOperationException MissingSubject() =>
        new("Verification subject must have a non-empty identity reference and claim type.");

    public static InvalidOperationException InvalidStateTransition(VerificationStatus status, string action) =>
        new($"Cannot perform '{action}' when verification status is '{status}'.");
}

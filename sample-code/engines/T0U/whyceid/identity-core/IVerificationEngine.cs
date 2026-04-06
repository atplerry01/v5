namespace Whycespace.Engines.T0U.WhyceId.Verification;

public interface IVerificationEngine
{
    VerificationDecisionResult Evaluate(VerificationDecisionCommand command);
}

public sealed record VerificationDecisionCommand(
    string IdentityId,
    string VerificationType,
    string Method,
    int CurrentAttempts,
    int MaxAttempts);

public sealed record VerificationDecisionResult(
    bool CanProceed,
    string? RejectionReason = null)
{
    public static VerificationDecisionResult Proceed() => new(true);
    public static VerificationDecisionResult Reject(string reason) => new(false, reason);
}

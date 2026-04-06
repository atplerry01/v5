namespace Whycespace.Engines.T0U.WhyceId.Verification;

public sealed class VerificationValidator
{
    public IdentityValidationResult Validate(VerificationDecisionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return IdentityValidationResult.Invalid("IdentityId is required.");
        if (string.IsNullOrWhiteSpace(command.VerificationType))
            return IdentityValidationResult.Invalid("VerificationType is required.");
        return IdentityValidationResult.Valid();
    }
}

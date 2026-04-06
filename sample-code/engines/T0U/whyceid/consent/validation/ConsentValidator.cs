namespace Whycespace.Engines.T0U.WhyceId.Consent;

public sealed class ConsentValidator
{
    public IdentityValidationResult Validate(ConsentDecisionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return IdentityValidationResult.Invalid("IdentityId is required.");
        if (string.IsNullOrWhiteSpace(command.ConsentType))
            return IdentityValidationResult.Invalid("ConsentType is required.");
        return IdentityValidationResult.Valid();
    }
}

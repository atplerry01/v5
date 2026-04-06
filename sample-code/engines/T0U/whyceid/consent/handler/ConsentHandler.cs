namespace Whycespace.Engines.T0U.WhyceId.Consent;

public sealed class ConsentHandler : IConsentEngine
{
    public ConsentDecisionResult Evaluate(ConsentDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return ConsentDecisionResult.Deny("IdentityId is required.");

        if (string.IsNullOrWhiteSpace(command.ConsentType))
            return ConsentDecisionResult.Deny("ConsentType is required.");

        if (command.HasExistingConsent)
            return ConsentDecisionResult.Deny("Consent already exists for this type and scope.");

        return ConsentDecisionResult.Permit();
    }
}

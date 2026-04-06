namespace Whycespace.Engines.T0U.WhyceId.Authentication;

public sealed class AuthenticationValidator
{
    public IdentityValidationResult Validate(AuthenticateCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return IdentityValidationResult.Invalid("IdentityId is required.");

        if (string.IsNullOrWhiteSpace(command.CredentialType))
            return IdentityValidationResult.Invalid("CredentialType is required.");

        if (string.IsNullOrWhiteSpace(command.CredentialValue))
            return IdentityValidationResult.Invalid("CredentialValue is required.");

        return IdentityValidationResult.Valid();
    }
}

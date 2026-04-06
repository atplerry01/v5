using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T0U.WhyceId.Authentication;

public sealed class AuthenticationHandler : IAuthenticationEngine
{
    public AuthenticationResult Authenticate(AuthenticateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return AuthenticationResult.Failure("IdentityId is required.");

        if (string.IsNullOrWhiteSpace(command.CredentialValue))
            return AuthenticationResult.Failure("Credential value is required.");

        // T0U: stateless decision — actual credential verification delegated to T2E
        return AuthenticationResult.Success(DeterministicIdHelper.FromSeed($"AuthSession:{command.IdentityId}:{command.CredentialType}:{command.DeviceId}").ToString("N"));
    }
}

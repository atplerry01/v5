namespace Whycespace.Engines.T0U.WhyceId.Authentication;

public interface IAuthenticationEngine
{
    AuthenticationResult Authenticate(AuthenticateCommand command);
}

public sealed record AuthenticateCommand(
    string IdentityId,
    string CredentialType,
    string CredentialValue,
    string DeviceId);

public sealed record AuthenticationResult(
    bool IsAuthenticated,
    string? FailureReason = null,
    string? SessionToken = null)
{
    public static AuthenticationResult Success(string sessionToken) => new(true, null, sessionToken);
    public static AuthenticationResult Failure(string reason) => new(false, reason);
}

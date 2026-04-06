namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public static class CredentialErrors
{
    public static string CredentialExpired => "Credential has expired and cannot be used.";
    public static string CredentialAlreadyRevoked => "Credential is already revoked.";
    public static string CredentialNotActive => "Credential is not in an active state.";
    public static string InvalidCredentialType => "The specified credential type is not supported.";
}

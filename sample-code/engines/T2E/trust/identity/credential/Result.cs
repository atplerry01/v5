namespace Whycespace.Engines.T2E.Trust.Identity.Credential;

public record CredentialResult(bool Success, string Message);
public sealed record CredentialDto(string CredentialId, string IdentityId, string CredentialType, string Status);

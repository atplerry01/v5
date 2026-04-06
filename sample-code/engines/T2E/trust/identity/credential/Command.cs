namespace Whycespace.Engines.T2E.Trust.Identity.Credential;

public record CredentialCommand(string Action, string EntityId, object Payload);
public sealed record IssueCredentialCommand(string IdentityId, string CredentialType, string ExpiryDate) : CredentialCommand("Issue", IdentityId, null!);
public sealed record RevokeCredentialCommand(string CredentialId) : CredentialCommand("Revoke", CredentialId, null!);
public sealed record RotateCredentialCommand(string CredentialId, string NewExpiryDate) : CredentialCommand("Rotate", CredentialId, null!);

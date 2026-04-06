namespace Whycespace.Engines.T2E.Trust.Identity.ServiceIdentity;

public record ServiceIdentityCommand(string Action, string EntityId, object Payload);
public sealed record RegisterServiceIdentityCommand(string ServiceName, string ServiceType) : ServiceIdentityCommand("Register", ServiceName, null!);
public sealed record IssueServiceCredentialCommand(string ServiceIdentityId, string ExpiresAt) : ServiceIdentityCommand("IssueCredential", ServiceIdentityId, null!);
public sealed record RevokeServiceCredentialCommand(string ServiceIdentityId, string CredentialId) : ServiceIdentityCommand("RevokeCredential", ServiceIdentityId, null!);
public sealed record SuspendServiceIdentityCommand(string ServiceIdentityId, string Reason) : ServiceIdentityCommand("Suspend", ServiceIdentityId, null!);
public sealed record ReactivateServiceIdentityCommand(string ServiceIdentityId) : ServiceIdentityCommand("Reactivate", ServiceIdentityId, null!);
public sealed record DecommissionServiceIdentityCommand(string ServiceIdentityId, string Reason) : ServiceIdentityCommand("Decommission", ServiceIdentityId, null!);

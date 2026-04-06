namespace Whycespace.Engines.T2E.Trust.Identity.Identity;

public record IdentityCommand(string Action, string EntityId, object Payload);
public sealed record CreateIdentityCommand(string IdentityId, string IdentityType, string DisplayName) : IdentityCommand("Create", IdentityId, null!);
public sealed record ActivateIdentityCommand(string IdentityId) : IdentityCommand("Activate", IdentityId, null!);
public sealed record SuspendIdentityCommand(string IdentityId, string Reason) : IdentityCommand("Suspend", IdentityId, null!);
public sealed record ReactivateIdentityCommand(string IdentityId) : IdentityCommand("Reactivate", IdentityId, null!);
public sealed record DeactivateIdentityCommand(string IdentityId, string Reason) : IdentityCommand("Deactivate", IdentityId, null!);
public sealed record UpdateIdentityDisplayNameCommand(string IdentityId, string NewDisplayName) : IdentityCommand("UpdateDisplayName", IdentityId, null!);

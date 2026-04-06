namespace Whycespace.Engines.T0U.WhyceId;

/// <summary>
/// Manifest of all WhyceID engine command types and their routing metadata.
/// The host layer uses this to create TypedEngineAdapters and register them
/// with EngineResolver during startup. The runtime does NOT reference T2E
/// directly — engines are wired by the host via DI.
/// </summary>
public static class IdentityEngineManifest
{
    public static IReadOnlyList<IdentityEngineEntry> GetEntries() =>
    [
        // Identity lifecycle
        new("identity.create", "Identity.Create", "1.0"),
        new("identity.activate", "Identity.Activate", "1.0"),
        new("identity.suspend", "Identity.Suspend", "1.0"),
        new("identity.reactivate", "Identity.Reactivate", "1.0"),
        new("identity.deactivate", "Identity.Deactivate", "1.0"),
        new("identity.update-display-name", "Identity.UpdateDisplayName", "1.0"),

        // Credential
        new("credential.issue", "Credential.Issue", "1.0"),
        new("credential.revoke", "Credential.Revoke", "1.0"),
        new("credential.rotate", "Credential.Rotate", "1.0"),

        // Role
        new("role.create", "Role.Create", "1.0"),
        new("role.assign", "Role.Assign", "1.0"),
        new("role.revoke", "Role.Revoke", "1.0"),
        new("role.deactivate", "Role.Deactivate", "1.0"),

        // Permission
        new("permission.create", "Permission.Create", "1.0"),
        new("permission.grant", "Permission.Grant", "1.0"),
        new("permission.revoke", "Permission.Revoke", "1.0"),
        new("permission.deactivate", "Permission.Deactivate", "1.0"),

        // Trust
        new("trust.initialize", "Trust.Initialize", "1.0"),
        new("trust.record-factor", "Trust.RecordFactor", "1.0"),
        new("trust.freeze", "Trust.Freeze", "1.0"),
        new("trust.unfreeze", "Trust.Unfreeze", "1.0"),

        // Verification
        new("verification.create", "Verification.Create", "1.0"),
        new("verification.add-attempt", "Verification.AddAttempt", "1.0"),
        new("verification.complete", "Verification.Complete", "1.0"),
        new("verification.fail", "Verification.Fail", "1.0"),
        new("verification.expire", "Verification.Expire", "1.0"),

        // Consent
        new("consent.grant", "Consent.Grant", "1.0"),
        new("consent.revoke", "Consent.Revoke", "1.0"),
        new("consent.expire", "Consent.Expire", "1.0"),

        // Session
        new("session.start", "Session.Start", "1.0"),
        new("session.refresh", "Session.Refresh", "1.0"),
        new("session.revoke", "Session.Revoke", "1.0"),
        new("session.expire", "Session.Expire", "1.0"),

        // Device
        new("device.register", "Device.Register", "1.0"),
        new("device.verify", "Device.Verify", "1.0"),
        new("device.block", "Device.Block", "1.0"),
        new("device.unblock", "Device.Unblock", "1.0"),
        new("device.deregister", "Device.Deregister", "1.0"),

        // ServiceIdentity
        new("service-identity.register", "ServiceIdentity.Register", "1.0"),
        new("service-identity.issue-credential", "ServiceIdentity.IssueCredential", "1.0"),
        new("service-identity.revoke-credential", "ServiceIdentity.RevokeCredential", "1.0"),
        new("service-identity.suspend", "ServiceIdentity.Suspend", "1.0"),
        new("service-identity.reactivate", "ServiceIdentity.Reactivate", "1.0"),
        new("service-identity.decommission", "ServiceIdentity.Decommission", "1.0"),

        // IdentityGraph
        new("identity-graph.create", "IdentityGraph.Create", "1.0"),
        new("identity-graph.link", "IdentityGraph.Link", "1.0"),
        new("identity-graph.unlink", "IdentityGraph.Unlink", "1.0"),
        new("identity-graph.merge", "IdentityGraph.Merge", "1.0"),
        new("identity-graph.close", "IdentityGraph.Close", "1.0"),

        // AccessProfile
        new("access-profile.create", "AccessProfile.Create", "1.0"),
        new("access-profile.add-role", "AccessProfile.AddRole", "1.0"),
        new("access-profile.remove-role", "AccessProfile.RemoveRole", "1.0"),
        new("access-profile.add-permission", "AccessProfile.AddPermission", "1.0"),
        new("access-profile.remove-permission", "AccessProfile.RemovePermission", "1.0"),
        new("access-profile.suspend", "AccessProfile.Suspend", "1.0"),
        new("access-profile.reactivate", "AccessProfile.Reactivate", "1.0"),
    ];
}

/// <summary>
/// Describes a single identity engine registration entry.
/// Used by the host layer to create TypedEngineAdapter instances
/// and register them with the EngineResolver.
/// </summary>
public sealed record IdentityEngineEntry(
    string CommandType,
    string EngineName,
    string Version);

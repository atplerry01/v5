using Whycespace.Shared.Contracts.Event;

namespace Whycespace.Shared.Contracts.Event;

/// <summary>
/// Registers identity domain event schemas against the shared IEventSchemaRegistry contract.
/// Moved from infrastructure/adapters — infrastructure must not reference domain.
/// </summary>
public static class IdentityEventSchemaRegistration
{
    public static void Register(IEventSchemaRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Identity.IdentityRegisteredEvent", 1, ["IdentityId", "IdentityType", "DisplayName"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Identity.IdentityActivatedEvent", 1, ["IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Identity.IdentitySuspendedEvent", 1, ["IdentityId", "Reason"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Identity.IdentityReactivatedEvent", 1, ["IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Identity.IdentityDeactivatedEvent", 1, ["IdentityId", "Reason"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Identity.IdentityProfileUpdatedEvent", 1, ["IdentityId", "OldDisplayName", "NewDisplayName"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Credential.CredentialIssuedEvent", 1, ["CredentialId", "IdentityId", "CredentialType", "IssuedAt", "ExpiryDate"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Credential.CredentialRevokedEvent", 1, ["CredentialId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Credential.CredentialRotatedEvent", 1, ["CredentialId", "NewCredentialId", "IdentityId", "CredentialType", "NewExpiryDate"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Consent.ConsentGrantedEvent", 1, ["ConsentId", "IdentityId", "ConsentType", "Scope"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Consent.ConsentRevokedEvent", 1, ["ConsentId", "IdentityId", "Reason"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Consent.ConsentExpiredEvent", 1, ["ConsentId", "IdentityId"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Session.SessionStartedEvent", 1, ["SessionId", "IdentityId", "DeviceId", "ExpiresAt"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Session.SessionRefreshedEvent", 1, ["SessionId", "NewExpiresAt", "LastAccessedAt"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Session.SessionRevokedEvent", 1, ["SessionId", "RevokedAt"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Session.SessionExpiredEvent", 1, ["SessionId", "ExpiredAt"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Role.RoleCreatedEvent", 1, ["RoleId", "Name", "Scope"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Role.RoleAssignedEvent", 1, ["RoleId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Role.RoleRevokedEvent", 1, ["RoleId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Role.RoleDeactivatedEvent", 1, ["RoleId"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Permission.PermissionCreatedEvent", 1, ["PermissionId", "Name", "Action", "Resource"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Permission.PermissionGrantedEvent", 1, ["PermissionId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Permission.PermissionRevokedEvent", 1, ["PermissionId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Permission.PermissionDeactivatedEvent", 1, ["PermissionId"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Trust.TrustProfileInitializedEvent", 1, ["TrustProfileId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Trust.TrustFactorRecordedEvent", 1, ["TrustProfileId", "IdentityId", "Factor", "Weight"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Trust.TrustScoreUpdatedEvent", 1, ["TrustProfileId", "IdentityId", "OldScore", "NewScore", "NewLevel"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Trust.TrustProfileFrozenEvent", 1, ["TrustProfileId", "IdentityId", "Reason"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Trust.TrustProfileUnfrozenEvent", 1, ["TrustProfileId", "IdentityId"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Verification.VerificationCreatedEvent", 1, ["VerificationId", "IdentityId", "VerificationType", "Method"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Verification.VerificationAttemptedEvent", 1, ["VerificationId", "IdentityId", "AttemptNumber"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Verification.VerificationCompletedEvent", 1, ["VerificationId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Verification.VerificationFailedEvent", 1, ["VerificationId", "IdentityId", "Reason"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Verification.VerificationExpiredEvent", 1, ["VerificationId", "IdentityId"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Device.DeviceRegisteredEvent", 1, ["DeviceId", "IdentityId", "DeviceType", "Fingerprint"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Device.DeviceVerifiedEvent", 1, ["DeviceId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Device.DeviceBlockedEvent", 1, ["DeviceId", "IdentityId", "Reason"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Device.DeviceUnblockedEvent", 1, ["DeviceId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Device.DeviceDeregisteredEvent", 1, ["DeviceId", "IdentityId", "Reason"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.ServiceIdentity.ServiceIdentityRegisteredEvent", 1, ["ServiceIdentityId", "ServiceName", "ServiceType"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.ServiceIdentity.ServiceCredentialIssuedEvent", 1, ["ServiceIdentityId", "CredentialId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.ServiceIdentity.ServiceCredentialRevokedEvent", 1, ["ServiceIdentityId", "CredentialId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.ServiceIdentity.ServiceIdentitySuspendedEvent", 1, ["ServiceIdentityId", "Reason"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.ServiceIdentity.ServiceIdentityReactivatedEvent", 1, ["ServiceIdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.ServiceIdentity.ServiceIdentityDecommissionedEvent", 1, ["ServiceIdentityId", "Reason"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.IdentityGraph.IdentityGraphCreatedEvent", 1, ["GraphId", "PrimaryIdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.IdentityGraph.IdentityLinkedEvent", 1, ["GraphId", "SourceIdentityId", "TargetIdentityId", "LinkType", "Strength"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.IdentityGraph.IdentityUnlinkedEvent", 1, ["GraphId", "SourceIdentityId", "TargetIdentityId", "LinkType"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.IdentityGraph.IdentityGraphMergedEvent", 1, ["TargetGraphId", "SourceGraphId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.IdentityGraph.IdentityGraphClosedEvent", 1, ["GraphId"]);

        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Profile.AccessProfileCreatedEvent", 1, ["ProfileId", "IdentityId", "AccessLevel"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Profile.RoleAddedToProfileEvent", 1, ["ProfileId", "RoleId", "RoleName"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Profile.RoleRemovedFromProfileEvent", 1, ["ProfileId", "RoleId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Profile.PermissionAddedToProfileEvent", 1, ["ProfileId", "PermissionId", "PermissionName"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Profile.PermissionRemovedFromProfileEvent", 1, ["ProfileId", "PermissionId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Profile.AccessProfileSuspendedEvent", 1, ["ProfileId", "IdentityId"]);
        RegisterEvent(registry, "Whycespace.Domain.TrustSystem.Identity.Profile.AccessProfileReactivatedEvent", 1, ["ProfileId", "IdentityId"]);
    }

    private static void RegisterEvent(IEventSchemaRegistry registry, string schemaId, int version, string[] fieldNames)
    {
        var fields = fieldNames.Select(f => new EventFieldDescriptorRecord
        {
            Name = f,
            TypeName = "System.String",
            IsRequired = true
        }).ToList();

        registry.Register(new EventSchemaRegistrationRecord
        {
            SchemaId = schemaId,
            Version = version,
            EventClrType = typeof(object),
            Schema = new EventSchemaVersionRecord
            {
                SchemaId = schemaId,
                Version = version,
                Fields = fields
            }
        });
    }
}

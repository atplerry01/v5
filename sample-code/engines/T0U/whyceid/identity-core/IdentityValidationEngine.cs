namespace Whycespace.Engines.T0U.WhyceId.IdentityCore;

/// <summary>
/// T0U decision: validates whether identity operations may proceed.
/// FAIL CLOSED — invalid input blocks execution.
/// Stateless, no domain imports, works with primitive types only.
/// </summary>
public sealed class IdentityValidationEngine : IdentityEngineBase
{
    private static readonly int MinDisplayNameLength = 2;
    private static readonly int MaxDisplayNameLength = 256;

    private static readonly HashSet<string> ValidIdentityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Individual", "Organization", "Service", "System"
    };

    private static readonly HashSet<string> ValidCredentialTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "APIKey", "Certificate", "OAuth"
    };

    private static readonly HashSet<string> ValidVerificationTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email", "Phone", "KYC", "Document", "Biometric"
    };

    private static readonly HashSet<string> ValidConsentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "KYC", "DataProcessing", "Marketing", "ThirdPartySharing", "Analytics"
    };

    private static readonly HashSet<string> ValidDeviceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Mobile", "Desktop", "Tablet", "IoT", "Browser"
    };

    private static readonly HashSet<string> ValidServiceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "API", "Worker", "Gateway", "Integration"
    };

    public IdentityValidationResult ValidateIdentityCreate(ValidateIdentityCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return IdentityValidationResult.Invalid("IdentityId is required.");

        if (!Guid.TryParse(command.IdentityId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("IdentityId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateIdentityCreate(string identityId, string identityType, string displayName)
    {
        if (string.IsNullOrWhiteSpace(identityId))
            return IdentityValidationResult.Invalid("IdentityId is required.");

        if (!Guid.TryParse(identityId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("IdentityId must be a valid non-empty GUID.");

        if (string.IsNullOrWhiteSpace(identityType))
            return IdentityValidationResult.Invalid("IdentityType is required.");

        if (!ValidIdentityTypes.Contains(identityType))
            return IdentityValidationResult.Invalid(
                $"IdentityType '{identityType}' is not valid. Must be one of: {string.Join(", ", ValidIdentityTypes)}.");

        if (string.IsNullOrWhiteSpace(displayName))
            return IdentityValidationResult.Invalid("DisplayName is required.");

        if (displayName.Length < MinDisplayNameLength || displayName.Length > MaxDisplayNameLength)
            return IdentityValidationResult.Invalid(
                $"DisplayName must be between {MinDisplayNameLength} and {MaxDisplayNameLength} characters.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidatePermissionCreate(ValidatePermissionCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.PermissionId))
            return IdentityValidationResult.Invalid("PermissionId is required.");

        if (!Guid.TryParse(command.PermissionId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("PermissionId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidatePermissionCreate(string permissionId, string name, string action, string resource, string scope)
    {
        var baseResult = ValidatePermissionCreate(new ValidatePermissionCreateCommand(permissionId));
        if (!baseResult.IsValid) return baseResult;

        if (string.IsNullOrWhiteSpace(name))
            return IdentityValidationResult.Invalid("Permission name is required.");

        if (string.IsNullOrWhiteSpace(action))
            return IdentityValidationResult.Invalid("Permission action is required.");

        if (string.IsNullOrWhiteSpace(resource))
            return IdentityValidationResult.Invalid("Permission resource is required.");

        if (string.IsNullOrWhiteSpace(scope))
            return IdentityValidationResult.Invalid("Permission scope is required.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateRoleCreate(ValidateRoleCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.RoleId))
            return IdentityValidationResult.Invalid("RoleId is required.");

        if (!Guid.TryParse(command.RoleId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("RoleId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateRoleCreate(string roleId, string name, string scope)
    {
        var baseResult = ValidateRoleCreate(new ValidateRoleCreateCommand(roleId));
        if (!baseResult.IsValid) return baseResult;

        if (string.IsNullOrWhiteSpace(name))
            return IdentityValidationResult.Invalid("Role name is required.");

        if (name.Length > 128)
            return IdentityValidationResult.Invalid("Role name must not exceed 128 characters.");

        if (string.IsNullOrWhiteSpace(scope))
            return IdentityValidationResult.Invalid("Role scope is required.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateTrustCreate(ValidateTrustCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.TrustId))
            return IdentityValidationResult.Invalid("TrustId (IdentityId) is required.");

        if (!Guid.TryParse(command.TrustId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("TrustId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateVerificationCreate(ValidateVerificationCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.VerificationId))
            return IdentityValidationResult.Invalid("VerificationId is required.");

        if (!Guid.TryParse(command.VerificationId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("VerificationId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateVerificationCreate(string verificationId, string identityId, string verificationType, string method)
    {
        var baseResult = ValidateVerificationCreate(new ValidateVerificationCreateCommand(verificationId));
        if (!baseResult.IsValid) return baseResult;

        if (string.IsNullOrWhiteSpace(identityId))
            return IdentityValidationResult.Invalid("IdentityId is required for verification.");

        if (string.IsNullOrWhiteSpace(verificationType))
            return IdentityValidationResult.Invalid("VerificationType is required.");

        if (!ValidVerificationTypes.Contains(verificationType))
            return IdentityValidationResult.Invalid(
                $"VerificationType '{verificationType}' is not valid. Must be one of: {string.Join(", ", ValidVerificationTypes)}.");

        if (string.IsNullOrWhiteSpace(method))
            return IdentityValidationResult.Invalid("Verification method is required.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateConsentCreate(ValidateConsentCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.ConsentId))
            return IdentityValidationResult.Invalid("ConsentId is required.");

        if (!Guid.TryParse(command.ConsentId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("ConsentId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateConsentCreate(string consentId, string identityId, string consentType, string scope)
    {
        var baseResult = ValidateConsentCreate(new ValidateConsentCreateCommand(consentId));
        if (!baseResult.IsValid) return baseResult;

        if (string.IsNullOrWhiteSpace(identityId))
            return IdentityValidationResult.Invalid("IdentityId is required for consent.");

        if (string.IsNullOrWhiteSpace(consentType))
            return IdentityValidationResult.Invalid("ConsentType is required.");

        if (!ValidConsentTypes.Contains(consentType))
            return IdentityValidationResult.Invalid(
                $"ConsentType '{consentType}' is not valid. Must be one of: {string.Join(", ", ValidConsentTypes)}.");

        if (string.IsNullOrWhiteSpace(scope))
            return IdentityValidationResult.Invalid("Consent scope is required.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateSessionCreate(ValidateSessionCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.SessionId))
            return IdentityValidationResult.Invalid("SessionId is required.");

        if (!Guid.TryParse(command.SessionId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("SessionId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateDeviceCreate(ValidateDeviceCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.DeviceId))
            return IdentityValidationResult.Invalid("DeviceId is required.");

        if (!Guid.TryParse(command.DeviceId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("DeviceId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateDeviceCreate(string deviceId, string identityId, string deviceType, string fingerprint)
    {
        var baseResult = ValidateDeviceCreate(new ValidateDeviceCreateCommand(deviceId));
        if (!baseResult.IsValid) return baseResult;

        if (string.IsNullOrWhiteSpace(identityId))
            return IdentityValidationResult.Invalid("IdentityId is required for device registration.");

        if (string.IsNullOrWhiteSpace(deviceType))
            return IdentityValidationResult.Invalid("DeviceType is required.");

        if (!ValidDeviceTypes.Contains(deviceType))
            return IdentityValidationResult.Invalid(
                $"DeviceType '{deviceType}' is not valid. Must be one of: {string.Join(", ", ValidDeviceTypes)}.");

        if (string.IsNullOrWhiteSpace(fingerprint))
            return IdentityValidationResult.Invalid("Device fingerprint is required.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateServiceIdentityCreate(ValidateServiceIdentityCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.ServiceIdentityId))
            return IdentityValidationResult.Invalid("ServiceIdentityId is required.");

        if (!Guid.TryParse(command.ServiceIdentityId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("ServiceIdentityId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateServiceIdentityCreate(string serviceIdentityId, string serviceName, string serviceType)
    {
        var baseResult = ValidateServiceIdentityCreate(new ValidateServiceIdentityCreateCommand(serviceIdentityId));
        if (!baseResult.IsValid) return baseResult;

        if (string.IsNullOrWhiteSpace(serviceName))
            return IdentityValidationResult.Invalid("ServiceName is required.");

        if (serviceName.Length > 256)
            return IdentityValidationResult.Invalid("ServiceName must not exceed 256 characters.");

        if (string.IsNullOrWhiteSpace(serviceType))
            return IdentityValidationResult.Invalid("ServiceType is required.");

        if (!ValidServiceTypes.Contains(serviceType))
            return IdentityValidationResult.Invalid(
                $"ServiceType '{serviceType}' is not valid. Must be one of: {string.Join(", ", ValidServiceTypes)}.");

        return IdentityValidationResult.Valid();
    }

    public IdentityValidationResult ValidateIdentityGraphCreate(ValidateIdentityGraphCreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.GraphId))
            return IdentityValidationResult.Invalid("GraphId is required.");

        if (!Guid.TryParse(command.GraphId, out var guid) || guid == Guid.Empty)
            return IdentityValidationResult.Invalid("GraphId must be a valid non-empty GUID.");

        return IdentityValidationResult.Valid();
    }
}

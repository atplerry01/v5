namespace Whycespace.Engines.T0U.WhyceId;

public abstract record IdentityCommand;

public sealed record VerifyIdentityCommand(string SubjectId, string Credential) : IdentityCommand;

public sealed record AuthorizeCommand(string SubjectId, string Resource, string Action) : IdentityCommand;

public sealed record ComputeTrustScoreCommand(string SubjectId) : IdentityCommand;

// Validation commands — T0U decides whether T2E may proceed
public sealed record ValidateIdentityCreateCommand(string IdentityId) : IdentityCommand;

public sealed record ValidatePermissionCreateCommand(string PermissionId) : IdentityCommand;

public sealed record ValidateRoleCreateCommand(string RoleId) : IdentityCommand;

public sealed record ValidateTrustCreateCommand(string TrustId) : IdentityCommand;

public sealed record ValidateVerificationCreateCommand(string VerificationId) : IdentityCommand;

public sealed record ValidateConsentCreateCommand(string ConsentId) : IdentityCommand;

public sealed record ValidateSessionCreateCommand(string SessionId) : IdentityCommand;

public sealed record ValidateDeviceCreateCommand(string DeviceId) : IdentityCommand;

public sealed record ValidateServiceIdentityCreateCommand(string ServiceIdentityId) : IdentityCommand;

public sealed record ValidateIdentityGraphCreateCommand(string GraphId) : IdentityCommand;

namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// Commands for T0U federation engines (constitutional).
/// ALL inputs are pre-parsed — no HTTP, no raw strings.
/// Uses string-based status values instead of domain enums.
/// </summary>
public abstract record FederationCommand;

// -- Validation --

public sealed record ValidateExternalIdentityCommand(
    string ExternalId,
    Guid IssuerId,
    string IssuerStatus,
    bool CredentialExpired,
    bool CredentialRevoked,
    bool ChainVerified) : FederationCommand;

// -- Governance --

public sealed record ApproveIssuerCommand(
    Guid IssuerId,
    string CurrentStatus,
    decimal TrustLevel,
    DateTimeOffset ApprovedAt) : FederationCommand;

public sealed record SuspendIssuerCommand(
    Guid IssuerId,
    string CurrentStatus,
    string Reason) : FederationCommand;

public sealed record RevokeIssuerCommand(
    Guid IssuerId,
    string CurrentStatus,
    string Reason) : FederationCommand;

// -- Policy Enforcement (policy-driven — no hardcoded thresholds) --

public sealed record EnforceFederationPolicyCommand(
    Guid IssuerId,
    decimal TrustLevel,
    decimal CurrentConfidence,
    string Provenance,
    string TrustStatus,
    FederationPolicyThresholds Thresholds) : FederationCommand;

/// <summary>
/// Policy thresholds provided by WHYCEPOLICY — engines NEVER hardcode these.
/// All enforcement decisions are driven by these explicit inputs.
/// </summary>
public sealed record FederationPolicyThresholds
{
    public required decimal MinTrust { get; init; }
    public required decimal MinConfidence { get; init; }
    public required string DegradedHandling { get; init; }     // "Deny" | "Conditional" | "Allow"
    public required string SystemInferredHandling { get; init; } // "Deny" | "Conditional" | "Allow"

    public static FederationPolicyThresholds Default => new()
    {
        MinTrust = 25m,
        MinConfidence = 0.3m,
        DegradedHandling = "Conditional",
        SystemInferredHandling = "Conditional"
    };
}

/// <summary>
/// Engine-local issuer status constants — decoupled from domain IssuerStatus enum.
/// </summary>
public static class IssuerStatusValues
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Suspended = "Suspended";
    public const string Revoked = "Revoked";
}

/// <summary>
/// Engine-local federation trust status constants.
/// </summary>
public static class FederationTrustStatusValues
{
    public const string Active = "Active";
    public const string Degraded = "Degraded";
    public const string Suspended = "Suspended";
}

/// <summary>
/// Engine-local provenance source constants.
/// </summary>
public static class ProvenanceSourceValues
{
    public const string UserVerified = "UserVerified";
    public const string SystemInferred = "SystemInferred";
}

namespace Whyce.Shared.Contracts.Runtime;

/// <summary>
/// Execution context for a command. Carries identity, policy, and correlation metadata.
///
/// T0U HARDENING:
/// - IdentityId, Roles, TrustScore are write-once (set by PolicyMiddleware during WhyceId resolution)
/// - PolicyDecisionAllowed, PolicyDecisionHash, PolicyVersion are write-once (set after WhycePolicy evaluation)
/// - Once set, these values are LOCKED for the remainder of the execution to ensure replay determinism
/// </summary>
public sealed record CommandContext
{
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
    public required Guid CommandId { get; init; }
    public required string TenantId { get; init; }
    public required string ActorId { get; init; }
    public required Guid AggregateId { get; init; }
    public required string PolicyId { get; init; }
    public bool RuntimeOrigin { get; set; }

    // Domain routing metadata — used for canonical Kafka topic resolution
    public required string Classification { get; init; }
    public required string Context { get; init; }
    public required string Domain { get; init; }

    // --- T0U Identity (write-once, locked after PolicyMiddleware) ---

    private string? _identityId;
    public string? IdentityId
    {
        get => _identityId;
        set
        {
            if (_identityId is not null)
                throw new InvalidOperationException(
                    "IdentityId is write-once. Already set to '" + _identityId + "'. " +
                    "Trust score, identity, and roles are locked after initial resolution.");
            _identityId = value;
        }
    }

    private string[]? _roles;
    public string[]? Roles
    {
        get => _roles;
        set
        {
            if (_roles is not null)
                throw new InvalidOperationException(
                    "Roles is write-once. Already locked after identity resolution.");
            _roles = value;
        }
    }

    private int? _trustScore;
    public int? TrustScore
    {
        get => _trustScore;
        set
        {
            if (_trustScore is not null)
                throw new InvalidOperationException(
                    "TrustScore is write-once. Already locked to " + _trustScore + ". " +
                    "Trust score is computed ONCE per request and never recomputed during execution.");
            _trustScore = value;
        }
    }

    // --- T0U Policy (write-once, locked after PolicyMiddleware) ---

    private bool? _policyDecisionAllowed;
    public bool? PolicyDecisionAllowed
    {
        get => _policyDecisionAllowed;
        set
        {
            if (_policyDecisionAllowed is not null)
                throw new InvalidOperationException(
                    "PolicyDecisionAllowed is write-once. Already locked after policy evaluation.");
            _policyDecisionAllowed = value;
        }
    }

    private string? _policyDecisionHash;
    public string? PolicyDecisionHash
    {
        get => _policyDecisionHash;
        set
        {
            if (_policyDecisionHash is not null)
                throw new InvalidOperationException(
                    "PolicyDecisionHash is write-once. Already locked after policy evaluation.");
            _policyDecisionHash = value;
        }
    }

    private string? _policyVersion;
    public string? PolicyVersion
    {
        get => _policyVersion;
        set
        {
            if (_policyVersion is not null)
                throw new InvalidOperationException(
                    "PolicyVersion is write-once. Already locked to '" + _policyVersion + "'. " +
                    "Replay uses the SAME version that was evaluated.");
            _policyVersion = value;
        }
    }
}

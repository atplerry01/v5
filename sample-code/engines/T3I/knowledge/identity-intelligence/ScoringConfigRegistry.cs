using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Whycespace.Engines.T3I.IdentityIntelligence;

/// <summary>
/// Registry of versioned scoring configurations.
/// Each config is hashed at registration — the hash guarantees no silent changes.
///
/// During replay, the same version MUST be used to reproduce identical outputs.
/// Engines MUST resolve their config from this registry; hardcoded values are forbidden.
/// Uses engine-local types instead of domain ScoringConfig/ScoringVersion.
/// </summary>
public sealed class ScoringConfigRegistry
{
    private readonly Dictionary<string, (ScoringConfig Config, ScoringVersionInfo Version)> _versions = new();

    /// <summary>
    /// Register a scoring config under a version id.
    /// The config hash (SHA256) is computed at registration time.
    /// </summary>
    public ScoringVersionInfo Register(string versionId, ScoringConfig config, DateTimeOffset activatedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionId);
        ArgumentNullException.ThrowIfNull(config);

        var configHash = ComputeConfigHash(config);
        var version = new ScoringVersionInfo(versionId, configHash, activatedAt);
        _versions[versionId] = (config, version);
        return version;
    }

    public (ScoringConfig Config, ScoringVersionInfo Version) Resolve(string versionId)
    {
        if (!_versions.TryGetValue(versionId, out var entry))
            throw new InvalidOperationException($"Scoring version '{versionId}' not found in registry.");
        return entry;
    }

    public ScoringVersionInfo GetVersion(string versionId) => Resolve(versionId).Version;
    public ScoringConfig GetConfig(string versionId) => Resolve(versionId).Config;
    public bool HasVersion(string versionId) => _versions.ContainsKey(versionId);
    public IReadOnlyList<string> GetAllVersionIds() => _versions.Keys.ToList().AsReadOnly();

    /// <summary>
    /// SHA256 hash of the serialized scoring config.
    /// Guarantees: if any parameter changes, the hash changes.
    /// </summary>
    private static string ComputeConfigHash(ScoringConfig config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>
    /// Creates a registry pre-loaded with the canonical v1.0 config.
    /// </summary>
    public static ScoringConfigRegistry CreateDefault()
    {
        var registry = new ScoringConfigRegistry();
        registry.Register("v1.0", ScoringConfigs.V1_0, ScoringConfigs.V1_0_ActivatedAt);
        return registry;
    }
}

/// <summary>
/// Engine-local scoring version info — decoupled from domain ScoringVersion.
/// </summary>
public sealed record ScoringVersionInfo(string VersionId, string ConfigHash, DateTimeOffset ActivatedAt);

/// <summary>
/// Engine-local scoring config — decoupled from domain ScoringConfig.
/// Contains all parameters needed for trust and risk computation.
/// </summary>
public sealed record ScoringConfig
{
    // Trust
    public decimal VerificationMaxContribution { get; init; }
    public double VerificationRate { get; init; }
    public decimal DeviceTrustWeight { get; init; }
    public decimal ViolationBaseWeight { get; init; }
    public decimal AccountAgeMaxContribution { get; init; }
    public double AccountAgeRate { get; init; }

    // Risk
    public decimal FailedAuthWeight { get; init; }
    public int FailedAuthThreshold { get; init; }
    public decimal DeviceSwitchMaxContribution { get; init; }
    public double DeviceSwitchRate { get; init; }
    public int DeviceSwitchThreshold { get; init; }
    public decimal LoginFreqWeight { get; init; }
    public decimal LoginFreqThreshold { get; init; }
    public decimal ViolationRiskWeight { get; init; }

    // Decay
    public double DecayLambda { get; init; }

    // Anti-gaming
    public decimal AntiGamingImprovementThreshold { get; init; }
    public int AntiGamingDaysThreshold { get; init; }
    public decimal AntiGamingPenaltyFactor { get; init; }
}

/// <summary>
/// Canonical scoring configs. Each version is a frozen snapshot of all parameters.
/// New versions are added here; old versions are NEVER modified.
/// </summary>
public static class ScoringConfigs
{
    public static readonly DateTimeOffset V1_0_ActivatedAt = new(2026, 3, 31, 0, 0, 0, TimeSpan.Zero);

    public static readonly ScoringConfig V1_0 = new()
    {
        // Trust
        VerificationMaxContribution = 45m,
        VerificationRate = 0.8,
        DeviceTrustWeight = 18m,
        ViolationBaseWeight = 15m,
        AccountAgeMaxContribution = 12m,
        AccountAgeRate = 0.003,

        // Risk
        FailedAuthWeight = 25m,
        FailedAuthThreshold = 3,
        DeviceSwitchMaxContribution = 25m,
        DeviceSwitchRate = 0.15,
        DeviceSwitchThreshold = 5,
        LoginFreqWeight = 12m,
        LoginFreqThreshold = 3.0m,
        ViolationRiskWeight = 10m,

        // Decay
        DecayLambda = 0.05,

        // Anti-gaming
        AntiGamingImprovementThreshold = 20m,
        AntiGamingDaysThreshold = 3,
        AntiGamingPenaltyFactor = 0.5m
    };
}

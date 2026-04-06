namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Explicit scoring configuration. Every weight, decay lambda, threshold, and
/// anti-gaming parameter is defined here — no implicit logic allowed.
///
/// Engines MUST receive a ScoringConfig instance; they NEVER use hardcoded values.
/// During replay, the same ScoringConfig MUST be used to reproduce identical outputs.
/// </summary>
public sealed record ScoringConfig
{
    // -- Trust weights --
    public required decimal VerificationMaxContribution { get; init; }
    public required double VerificationRate { get; init; }
    public required decimal DeviceTrustWeight { get; init; }
    public required decimal ViolationBaseWeight { get; init; }
    public required decimal AccountAgeMaxContribution { get; init; }
    public required double AccountAgeRate { get; init; }

    // -- Risk weights --
    public required decimal FailedAuthWeight { get; init; }
    public required int FailedAuthThreshold { get; init; }
    public required decimal DeviceSwitchMaxContribution { get; init; }
    public required double DeviceSwitchRate { get; init; }
    public required int DeviceSwitchThreshold { get; init; }
    public required decimal LoginFreqWeight { get; init; }
    public required decimal LoginFreqThreshold { get; init; }
    public required decimal ViolationRiskWeight { get; init; }

    // -- Decay --
    public required double DecayLambda { get; init; }

    // -- Anti-gaming --
    public required decimal AntiGamingImprovementThreshold { get; init; }
    public required int AntiGamingDaysThreshold { get; init; }
    public required decimal AntiGamingPenaltyFactor { get; init; }
}

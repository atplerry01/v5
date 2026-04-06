using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Identity;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.IdentityIntelligence;

/// <summary>
/// T3I Risk Engine — versioned, deterministic risk scoring.
///
/// MUST use explicit ScoringVersion from ScoringConfigRegistry.
/// NEVER relies on implicit/hardcoded logic.
/// During replay, same version MUST produce identical outputs.
///
/// Stateless. No persistence. Advisory output only.
/// Uses IIdentityIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class RiskEngine
{
    private readonly IClock _clock;
    private readonly ScoringConfigRegistry _registry;
    private readonly IIdentityIntelligenceDomainService _domainService;

    public RiskEngine(IClock clock, ScoringConfigRegistry registry, IIdentityIntelligenceDomainService domainService)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<RiskResult> ComputeAsync(ComputeRiskCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!command.ChainVerified)
            throw new InvalidOperationException("CHAIN_VERIFICATION_REQUIRED: Risk engine rejects unverified input.");

        var (config, version) = _registry.Resolve(command.ScoringVersionId);

        var signals = command.BehaviorSignals
            .Select(s => (object)new { s.SignalType, s.Weight, s.ObservedAt })
            .ToList();

        var violations = command.PolicyViolations
            .Select(v => (object)new { v.ViolationType, v.OccurredAt })
            .ToList();

        // Create trust profile via domain service for risk assessment
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = DeterministicIdHelper.FromSeed($"{command.IdentityId}:ComputeRisk:{command.ScoringVersionId}").ToString("N"),
            ActorId = "system",
            Action = "ComputeRisk",
            Domain = "intelligence.identity",
            Timestamp = _clock.UtcNowOffset
        };

        await _domainService.CreateTrustProfileAsync(
            execCtx, Guid.Parse(command.IdentityId), signals, violations);

        // Compute risk score using config parameters (engine-side deterministic computation)
        var failedAuthScore = command.FailedAuthCount > config.FailedAuthThreshold
            ? config.FailedAuthWeight * Math.Min((decimal)command.FailedAuthCount / config.FailedAuthThreshold, 3m)
            : 0m;

        var deviceSwitchScore = command.DeviceSwitchesInWindow > config.DeviceSwitchThreshold
            ? config.DeviceSwitchMaxContribution *
              (decimal)(1.0 - Math.Exp(-((double)config.DeviceSwitchRate * command.DeviceSwitchesInWindow)))
            : 0m;

        var loginFreqScore = command.LoginFrequencyRatio > config.LoginFreqThreshold
            ? config.LoginFreqWeight
            : 0m;

        var violationScore = config.ViolationRiskWeight * command.PolicyViolations.Count;

        var graphScore = command.GraphAnomalyScore * 15m;

        var rawScore = Math.Clamp(
            failedAuthScore + deviceSwitchScore + loginFreqScore + violationScore + graphScore,
            0m, 100m);

        var severity = rawScore switch
        {
            >= 75m => "Critical",
            >= 50m => "High",
            >= 25m => "Medium",
            _ => "Low"
        };

        var flags = new List<AnomalyFlagDto>();
        if (failedAuthScore > 0)
            flags.Add(new AnomalyFlagDto("failed_auth", $"{command.FailedAuthCount} failed auth attempts", Math.Min(failedAuthScore / config.FailedAuthWeight, 1m)));
        if (deviceSwitchScore > 0)
            flags.Add(new AnomalyFlagDto("device_switch", $"{command.DeviceSwitchesInWindow} device switches", Math.Min(deviceSwitchScore / config.DeviceSwitchMaxContribution, 1m)));
        if (command.GraphAnomalyScore > 0.5m)
            flags.Add(new AnomalyFlagDto("graph_anomaly", $"Graph anomaly score: {command.GraphAnomalyScore:F2}", command.GraphAnomalyScore));

        var explanation = $"Risk computed: failedAuth={failedAuthScore:F1}, deviceSwitch={deviceSwitchScore:F1}, loginFreq={loginFreqScore:F1}, violations={violationScore:F1}, graph={graphScore:F1}";

        return new RiskResult(
            command.IdentityId,
            rawScore,
            severity,
            flags,
            version.VersionId,
            explanation,
            _clock.UtcNowOffset);
    }
}

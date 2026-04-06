using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Identity;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.IdentityIntelligence;

/// <summary>
/// T3I Trust Engine — versioned, deterministic trust scoring.
///
/// MUST use explicit ScoringVersion from ScoringConfigRegistry.
/// NEVER relies on implicit/hardcoded logic.
/// During replay, same version MUST produce identical outputs.
///
/// Stateless. No persistence. Deterministic.
/// Uses IIdentityIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class TrustEngine
{
    private readonly IClock _clock;
    private readonly ScoringConfigRegistry _registry;
    private readonly IIdentityIntelligenceDomainService _domainService;

    public TrustEngine(IClock clock, ScoringConfigRegistry registry, IIdentityIntelligenceDomainService domainService)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<TrustResult> ComputeAsync(ComputeTrustCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!command.ChainVerified)
            throw new InvalidOperationException("CHAIN_VERIFICATION_REQUIRED: Trust engine rejects unverified input.");

        var (config, version) = _registry.Resolve(command.ScoringVersionId);

        var signals = command.BehaviorSignals
            .Select(s => (object)new { s.SignalType, s.Weight, s.ObservedAt })
            .ToList();

        var violations = command.PolicyViolations
            .Select(v => (object)new { v.ViolationType, v.OccurredAt })
            .ToList();

        // Create trust profile via domain service
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = DeterministicIdHelper.FromSeed($"{command.IdentityId}:ComputeTrust:{command.ScoringVersionId}").ToString("N"),
            ActorId = "system",
            Action = "ComputeTrust",
            Domain = "intelligence.identity",
            Timestamp = _clock.UtcNowOffset
        };

        await _domainService.CreateTrustProfileAsync(
            execCtx, Guid.Parse(command.IdentityId), signals, violations);

        // Compute trust score using config parameters (engine-side deterministic computation)
        var verificationContribution = config.VerificationMaxContribution *
            (decimal)(1.0 - Math.Exp(-config.VerificationRate * command.VerificationLevel));
        var deviceContribution = config.DeviceTrustWeight * command.DeviceTrustFactor;
        var violationPenalty = config.ViolationBaseWeight * command.PolicyViolations.Count;
        var ageContribution = config.AccountAgeMaxContribution *
            (decimal)(1.0 - Math.Exp(-config.AccountAgeRate * command.AccountAgeDays));

        var rawScore = Math.Clamp(
            verificationContribution + deviceContribution - violationPenalty + ageContribution,
            0m, 100m);

        // Apply decay if previous score exists
        if (command.PreviousScore.HasValue && command.PreviousScoreAgeDays.HasValue)
        {
            var decayFactor = (decimal)Math.Exp(-config.DecayLambda * command.PreviousScoreAgeDays.Value);
            rawScore = rawScore * 0.7m + command.PreviousScore.Value * decayFactor * 0.3m;
            rawScore = Math.Clamp(rawScore, 0m, 100m);
        }

        var classification = rawScore switch
        {
            >= 75m => "High",
            >= 40m => "Medium",
            _ => "Low"
        };

        var explanation = $"Trust computed: verification={verificationContribution:F1}, device={deviceContribution:F1}, violations=-{violationPenalty:F1}, age={ageContribution:F1}";

        return new TrustResult(
            command.IdentityId,
            rawScore,
            classification,
            version.VersionId,
            explanation,
            _clock.UtcNowOffset);
    }
}

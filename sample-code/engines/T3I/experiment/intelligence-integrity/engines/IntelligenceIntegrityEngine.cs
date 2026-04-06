using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.IntelligenceIntegrity;

/// <summary>
/// T3I engine: validates cross-signal consistency, recalibrates confidence,
/// detects intelligence conflicts, produces unified integrity result.
/// Stateless, deterministic. No persistence, no HTTP, no system clock.
/// Uses IEconomicIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class IntelligenceIntegrityEngine : IEngine<EvaluateIntelligenceIntegrityCommand>
{
    private readonly IEconomicIntelligenceDomainService _domainService;

    public IntelligenceIntegrityEngine(IEconomicIntelligenceDomainService domainService)
    {
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<EngineResult> ExecuteAsync(
        EvaluateIntelligenceIntegrityCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(command);
        if (validation is not null)
            return EngineResult.Fail(validation, "INTEGRITY_VALIDATION_FAILED");

        // Step 1: Validate cross-signal consistency
        var (conflictDetected, conflictReason) = DetectConflict(command);

        // Step 2: Compute integrity score
        var integrityScore = ComputeIntegrityScore(
            command.DeviationPercentage,
            command.ForecastConfidence,
            command.AnomalyConfidence,
            command.OptimizationConfidence);

        // Step 3: Calibrate confidence
        var calibratedConfidence = CalibrateConfidence(
            conflictDetected,
            command.ForecastConfidence,
            command.AnomalyConfidence,
            command.OptimizationConfidence);

        // Post-computation boundary guards
        if (integrityScore < 0 || integrityScore > 1)
            return EngineResult.Fail(
                "Computed IntegrityScore out of bounds [0,1].", "INTEGRITY_SCORE_OUT_OF_BOUNDS");
        if (calibratedConfidence < 0 || calibratedConfidence > 1)
            return EngineResult.Fail(
                "Computed CalibratedConfidence out of bounds [0,1].", "CALIBRATED_CONFIDENCE_OUT_OF_BOUNDS");

        // Step 4: Deterministic ObservedAt
        var observedAt = command.WindowEnd;

        // Parse IDs
        var integrityId = Guid.Parse(command.IntegrityId);
        var identityId = Guid.Parse(command.IdentityId);
        Guid? walletId = command.WalletId is not null ? Guid.Parse(command.WalletId) : null;

        // Atomic aggregate creation via domain service
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = command.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "intelligence.economic",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = observedAt
        };

        await _domainService.CreateIntegrityCheckAsync(
            execCtx,
            integrityId, identityId, walletId, command.Scope,
            integrityScore, calibratedConfidence, conflictDetected, conflictReason,
            command.CorrelationId,
            observedAt, command.WindowStart, command.WindowEnd);

        // Build result DTO
        var result = new IntegrityResultDto
        {
            IntegrityId = command.IntegrityId,
            IdentityId = command.IdentityId,
            WalletId = command.WalletId,
            Scope = command.Scope,
            IntegrityScore = integrityScore,
            CalibratedConfidence = calibratedConfidence,
            ConflictDetected = conflictDetected,
            ConflictReason = conflictReason,
            WindowStart = command.WindowStart,
            WindowEnd = command.WindowEnd,
            ObservedAt = observedAt,
            CorrelationId = command.CorrelationId
        };

        return EngineResult.Ok(result);
    }

    private static (bool Detected, string? Reason) DetectConflict(
        EvaluateIntelligenceIntegrityCommand command)
    {
        if (command.DeviationPercentage > 0.5m && command.ForecastConfidence > 0.9m)
            return (true, "High confidence forecast contradicted by high anomaly");

        if (command.ExpectedImpact == 0m && command.DeviationPercentage > 0.3m)
            return (true, "Optimization impact does not reflect anomaly severity");

        return (false, null);
    }

    private static decimal ComputeIntegrityScore(
        decimal deviationPercentage,
        decimal forecastConfidence,
        decimal anomalyConfidence,
        decimal optimizationConfidence)
    {
        var deviationFactor = Math.Max(0m, 1m - deviationPercentage);
        return deviationFactor * forecastConfidence * anomalyConfidence * optimizationConfidence;
    }

    private static decimal CalibrateConfidence(
        bool conflictDetected,
        decimal forecastConfidence,
        decimal anomalyConfidence,
        decimal optimizationConfidence)
    {
        if (conflictDetected)
        {
            var min = Math.Min(forecastConfidence, Math.Min(anomalyConfidence, optimizationConfidence));
            return min * 0.5m;
        }

        return (forecastConfidence + anomalyConfidence + optimizationConfidence) / 3m;
    }

    private static string? Validate(EvaluateIntelligenceIntegrityCommand command)
    {
        if (command.WindowEnd <= command.WindowStart)
            return "WindowEnd must be after WindowStart.";
        if (command.Volume < 0)
            return "Volume cannot be negative.";
        if (command.Velocity < 0)
            return "Velocity cannot be negative.";
        if (command.TransactionCount < 0)
            return "TransactionCount cannot be negative.";
        if (command.PredictedValue < 0)
            return "PredictedValue cannot be negative.";
        if (command.DeviationPercentage < 0 || command.DeviationPercentage > 1)
            return "DeviationPercentage must be between 0 and 1.";
        if (command.ForecastConfidence < 0 || command.ForecastConfidence > 1)
            return "ForecastConfidence must be between 0 and 1.";
        if (command.AnomalyConfidence < 0 || command.AnomalyConfidence > 1)
            return "AnomalyConfidence must be between 0 and 1.";
        if (command.OptimizationConfidence < 0 || command.OptimizationConfidence > 1)
            return "OptimizationConfidence must be between 0 and 1.";
        if (string.IsNullOrWhiteSpace(command.IntegrityId))
            return "IntegrityId is required.";
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return "IdentityId is required.";
        if (string.IsNullOrWhiteSpace(command.Scope))
            return "Scope is required.";
        return null;
    }
}

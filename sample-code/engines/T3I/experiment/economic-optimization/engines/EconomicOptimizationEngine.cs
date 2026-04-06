using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Engines.T3I.EconomicAnomaly;

namespace Whycespace.Engines.T3I.EconomicOptimization;

/// <summary>
/// T3I engine: generates optimization recommendations from analysis, forecast, and anomaly data.
/// Stateless, deterministic. No persistence, no HTTP, no system clock.
/// Explainable: strategy selection and impact derived from fixed rules.
/// Uses IEconomicIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class EconomicOptimizationEngine : IEngine<GenerateEconomicOptimizationCommand>
{
    private readonly IEconomicIntelligenceDomainService _domainService;

    public EconomicOptimizationEngine(IEconomicIntelligenceDomainService domainService)
    {
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<EngineResult> ExecuteAsync(
        GenerateEconomicOptimizationCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(command);
        if (validation is not null)
            return EngineResult.Fail(validation, "OPTIMIZATION_VALIDATION_FAILED");

        // Step 1: Determine optimization strategy
        var (recommendationType, action) = DetermineStrategy(command.Severity);

        // Step 2: Compute expected impact (unified formula)
        var expectedImpact = command.DeviationPercentage * command.PredictedValue;

        // Step 3: Confidence score
        var confidenceValue = ResolveConfidence(command.Severity);

        // Step 4: Deterministic ObservedAt
        var observedAt = command.WindowEnd;

        // Parse IDs
        var optimizationId = Guid.Parse(command.OptimizationId);
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

        await _domainService.CreateOptimizationAsync(
            execCtx,
            optimizationId, identityId, walletId, command.Scope,
            recommendationType, action, expectedImpact, confidenceValue,
            command.CorrelationId, command.SourceEventId,
            observedAt, command.WindowStart, command.WindowEnd);

        // Build result DTO
        var result = new OptimizationResultDto
        {
            OptimizationId = command.OptimizationId,
            IdentityId = command.IdentityId,
            WalletId = command.WalletId,
            Scope = command.Scope,
            RecommendationType = recommendationType,
            RecommendationAction = action,
            ExpectedImpact = expectedImpact,
            ConfidenceScore = confidenceValue,
            WindowStart = command.WindowStart,
            WindowEnd = command.WindowEnd,
            ObservedAt = observedAt,
            CorrelationId = command.CorrelationId,
            SourceEventId = command.SourceEventId
        };

        return EngineResult.Ok(result);
    }

    private static (string Type, string Action) DetermineStrategy(string severity)
    {
        if (severity == SeverityLevels.Critical)
            return (RecommendationTypes.RiskMitigation,
                "Reduce exposure or restrict transactions");

        if (severity == SeverityLevels.High)
            return (RecommendationTypes.CostReduction,
                "Reduce spending rate");

        if (severity == SeverityLevels.Medium)
            return (RecommendationTypes.EfficiencyGain,
                "Optimize transaction patterns");

        return (RecommendationTypes.RevenueGrowth,
            "Increase utilization or expand activity");
    }

    private static decimal ResolveConfidence(string severity)
    {
        if (severity == SeverityLevels.Critical) return 0.95m;
        if (severity == SeverityLevels.High) return 0.85m;
        if (severity == SeverityLevels.Medium) return 0.75m;
        return 0.65m;
    }

    private static string? Validate(GenerateEconomicOptimizationCommand command)
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
        if (command.DeviationPercentage < 0)
            return "DeviationPercentage cannot be negative.";
        if (string.IsNullOrWhiteSpace(command.CorrelationId))
            return "CorrelationId is required.";
        if (string.IsNullOrWhiteSpace(command.SourceEventId))
            return "SourceEventId is required.";
        if (string.IsNullOrWhiteSpace(command.OptimizationId))
            return "OptimizationId is required.";
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return "IdentityId is required.";
        if (string.IsNullOrWhiteSpace(command.Scope))
            return "Scope is required.";
        if (string.IsNullOrWhiteSpace(command.ForecastType))
            return "ForecastType is required.";
        if (string.IsNullOrWhiteSpace(command.Severity))
            return "Severity is required.";
        return null;
    }
}

/// <summary>
/// Engine-local recommendation type constants — decoupled from domain RecommendationType.
/// </summary>
public static class RecommendationTypes
{
    public const string RiskMitigation = "RiskMitigation";
    public const string CostReduction = "CostReduction";
    public const string EfficiencyGain = "EfficiencyGain";
    public const string RevenueGrowth = "RevenueGrowth";
}

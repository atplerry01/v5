using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Engines.T3I.EconomicForecast;

namespace Whycespace.Engines.T3I.EconomicAnomaly;

/// <summary>
/// T3I engine: detects economic anomalies by comparing actual vs expected values.
/// Stateless, deterministic. No persistence, no HTTP, no system clock.
/// Explainable: severity and confidence derived from deviation thresholds.
/// Uses IEconomicIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class EconomicAnomalyEngine : IEngine<DetectEconomicAnomalyCommand>
{
    private readonly IEconomicIntelligenceDomainService _domainService;

    public EconomicAnomalyEngine(IEconomicIntelligenceDomainService domainService)
    {
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<EngineResult> ExecuteAsync(
        DetectEconomicAnomalyCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(command);
        if (validation is not null)
            return EngineResult.Fail(validation, "ANOMALY_VALIDATION_FAILED");

        // Step 1: Select actual value based on forecast type
        var actual = SelectActualValue(command);

        // Step 2: Compute deviation
        var deviation = Math.Abs(actual - command.ExpectedValue);

        // Step 3: Deviation percentage
        var deviationPercentage = command.ExpectedValue > 0
            ? deviation / command.ExpectedValue
            : 0m;

        // Step 4: Severity classification
        var severity = ClassifySeverity(deviationPercentage);

        // Step 5: Confidence score
        var confidenceValue = ResolveConfidence(severity);

        // Step 6: Deterministic ObservedAt
        var observedAt = command.WindowEnd;

        // Parse IDs
        var anomalyId = Guid.Parse(command.AnomalyId);
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

        await _domainService.CreateAnomalyAsync(
            execCtx,
            anomalyId, identityId, walletId, command.Scope,
            severity, confidenceValue, deviation, deviationPercentage,
            command.CorrelationId, command.SourceEventId,
            observedAt, command.WindowStart, command.WindowEnd);

        // Build result DTO
        var result = new AnomalyResultDto
        {
            AnomalyId = command.AnomalyId,
            IdentityId = command.IdentityId,
            WalletId = command.WalletId,
            Scope = command.Scope,
            Deviation = deviation,
            DeviationPercentage = deviationPercentage,
            Severity = severity,
            ConfidenceScore = confidenceValue,
            WindowStart = command.WindowStart,
            WindowEnd = command.WindowEnd,
            ObservedAt = observedAt,
            CorrelationId = command.CorrelationId,
            SourceEventId = command.SourceEventId
        };

        return EngineResult.Ok(result);
    }

    private static decimal SelectActualValue(DetectEconomicAnomalyCommand command)
    {
        if (command.ForecastType == ForecastTypes.Balance)
            return command.ActualVolume;

        if (command.ForecastType == ForecastTypes.Usage)
            return command.ActualVelocity;

        if (command.ForecastType == ForecastTypes.Revenue)
            return command.ActualVolume;

        return command.ActualVolume;
    }

    private static string ClassifySeverity(decimal deviationPercentage)
    {
        if (deviationPercentage >= 0.5m) return SeverityLevels.Critical;
        if (deviationPercentage >= 0.25m) return SeverityLevels.High;
        if (deviationPercentage >= 0.1m) return SeverityLevels.Medium;
        return SeverityLevels.Low;
    }

    private static decimal ResolveConfidence(string severity)
    {
        if (severity == SeverityLevels.Critical) return 0.95m;
        if (severity == SeverityLevels.High) return 0.85m;
        if (severity == SeverityLevels.Medium) return 0.75m;
        return 0.6m;
    }

    private static string? Validate(DetectEconomicAnomalyCommand command)
    {
        if (command.WindowEnd <= command.WindowStart)
            return "WindowEnd must be after WindowStart.";
        if (command.ActualVolume < 0)
            return "ActualVolume cannot be negative.";
        if (command.ActualVelocity < 0)
            return "ActualVelocity cannot be negative.";
        if (command.ActualTransactionCount < 0)
            return "ActualTransactionCount cannot be negative.";
        if (command.ExpectedValue < 0)
            return "ExpectedValue cannot be negative.";
        if (string.IsNullOrWhiteSpace(command.CorrelationId))
            return "CorrelationId is required.";
        if (string.IsNullOrWhiteSpace(command.SourceEventId))
            return "SourceEventId is required.";
        if (string.IsNullOrWhiteSpace(command.AnomalyId))
            return "AnomalyId is required.";
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return "IdentityId is required.";
        if (string.IsNullOrWhiteSpace(command.Scope))
            return "Scope is required.";
        if (string.IsNullOrWhiteSpace(command.ForecastType))
            return "ForecastType is required.";
        return null;
    }
}

/// <summary>
/// Engine-local severity level constants — decoupled from domain SeverityLevel.
/// </summary>
public static class SeverityLevels
{
    public const string Critical = "Critical";
    public const string High = "High";
    public const string Medium = "Medium";
    public const string Low = "Low";
}

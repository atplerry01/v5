using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.EconomicForecast;

/// <summary>
/// T3I engine: generates economic forecasts from analysis results.
/// Stateless, deterministic. No persistence, no HTTP, no system clock.
/// ObservedAt derived as WindowEnd. HorizonSeconds derived from TimeHorizon.
/// Uses IEconomicIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class EconomicForecastEngine : IEngine<GenerateEconomicForecastCommand>
{
    private const decimal ShortHorizonSeconds = 3600m;       // 1 hour
    private const decimal MediumHorizonSeconds = 86400m;     // 24 hours
    private const decimal LongHorizonSeconds = 604800m;      // 7 days

    private const decimal ShortConfidence = 0.9m;
    private const decimal MediumConfidence = 0.7m;
    private const decimal LongConfidence = 0.5m;

    private readonly IEconomicIntelligenceDomainService _domainService;

    public EconomicForecastEngine(IEconomicIntelligenceDomainService domainService)
    {
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<EngineResult> ExecuteAsync(
        GenerateEconomicForecastCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(command);
        if (validation is not null)
            return EngineResult.Fail(validation, "FORECAST_VALIDATION_FAILED");

        // Step 1: Determine horizon seconds
        var horizonSeconds = ResolveHorizonSeconds(command.TimeHorizon);

        // Step 2: Compute base rate
        var windowSeconds = (decimal)(command.WindowEnd - command.WindowStart).TotalSeconds;
        var baseRate = windowSeconds > 0 ? command.Volume / windowSeconds : 0m;

        // Step 3: Predict value
        var predictedValue = ComputePrediction(
            command.ForecastType, baseRate, command.Velocity,
            command.AverageTransactionSize, command.TransactionCount, horizonSeconds);

        // Step 4: Confidence score
        var confidenceValue = ResolveConfidence(command.TimeHorizon);

        // Step 5: Deterministic ObservedAt
        var observedAt = command.WindowEnd;

        // Parse IDs
        var forecastId = Guid.Parse(command.ForecastId);
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

        await _domainService.CreateForecastAsync(
            execCtx,
            forecastId, identityId, walletId, command.Scope,
            command.ForecastType, command.TimeHorizon,
            predictedValue, confidenceValue,
            command.CorrelationId, command.SourceEventId,
            observedAt, command.WindowStart, command.WindowEnd);

        // Build result DTO
        var result = new ForecastResultDto
        {
            ForecastId = command.ForecastId,
            IdentityId = command.IdentityId,
            WalletId = command.WalletId,
            Scope = command.Scope,
            ForecastType = command.ForecastType,
            TimeHorizon = command.TimeHorizon,
            PredictedValue = predictedValue,
            ConfidenceScore = confidenceValue,
            WindowStart = command.WindowStart,
            WindowEnd = command.WindowEnd,
            ObservedAt = observedAt,
            CorrelationId = command.CorrelationId,
            SourceEventId = command.SourceEventId
        };

        return EngineResult.Ok(result);
    }

    private static decimal ComputePrediction(
        string forecastType,
        decimal baseRate,
        decimal velocity,
        decimal averageTransactionSize,
        int transactionCount,
        decimal horizonSeconds)
    {
        if (forecastType == ForecastTypes.Balance)
            return baseRate * horizonSeconds;

        if (forecastType == ForecastTypes.Usage)
            return velocity * horizonSeconds;

        if (forecastType == ForecastTypes.Revenue)
            return averageTransactionSize * transactionCount;

        return 0m;
    }

    private static decimal ResolveHorizonSeconds(string horizon)
    {
        if (horizon == TimeHorizons.Short) return ShortHorizonSeconds;
        if (horizon == TimeHorizons.Medium) return MediumHorizonSeconds;
        if (horizon == TimeHorizons.Long) return LongHorizonSeconds;
        return ShortHorizonSeconds;
    }

    private static decimal ResolveConfidence(string horizon)
    {
        if (horizon == TimeHorizons.Short) return ShortConfidence;
        if (horizon == TimeHorizons.Medium) return MediumConfidence;
        if (horizon == TimeHorizons.Long) return LongConfidence;
        return ShortConfidence;
    }

    private static string? Validate(GenerateEconomicForecastCommand command)
    {
        if (command.WindowEnd <= command.WindowStart)
            return "WindowEnd must be after WindowStart.";
        if (command.Volume < 0)
            return "Volume cannot be negative.";
        if (command.Velocity < 0)
            return "Velocity cannot be negative.";
        if (command.TransactionCount < 0)
            return "TransactionCount cannot be negative.";
        if (command.AverageTransactionSize < 0)
            return "AverageTransactionSize cannot be negative.";
        if (string.IsNullOrWhiteSpace(command.CorrelationId))
            return "CorrelationId is required.";
        if (string.IsNullOrWhiteSpace(command.SourceEventId))
            return "SourceEventId is required.";
        if (string.IsNullOrWhiteSpace(command.ForecastId))
            return "ForecastId is required.";
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return "IdentityId is required.";
        if (string.IsNullOrWhiteSpace(command.Scope))
            return "Scope is required.";
        if (string.IsNullOrWhiteSpace(command.ForecastType))
            return "ForecastType is required.";
        if (string.IsNullOrWhiteSpace(command.TimeHorizon))
            return "TimeHorizon is required.";
        return null;
    }
}

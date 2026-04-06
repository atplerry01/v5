using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.EconomicAnalysis;

/// <summary>
/// T3I engine: computes economic analysis metrics from projection data.
/// Stateless, deterministic. No persistence, no HTTP, no system clock.
/// ObservedAt is derived as WindowEnd (deterministic, no clock dependency).
/// Uses IEconomicIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class EconomicAnalysisEngine : IEngine<AnalyzeEconomicCommand>
{
    private readonly IEconomicIntelligenceDomainService _domainService;

    public EconomicAnalysisEngine(IEconomicIntelligenceDomainService domainService)
    {
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<EngineResult> ExecuteAsync(
        AnalyzeEconomicCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        // Validation
        var validation = Validate(command);
        if (validation is not null)
            return EngineResult.Fail(validation, "ANALYSIS_VALIDATION_FAILED");

        // Compute metrics (deterministic)
        var volume = command.TotalCredits + command.TotalDebits;

        var windowSeconds = (decimal)(command.WindowEnd - command.WindowStart).TotalSeconds;
        var velocity = windowSeconds > 0
            ? command.TransactionCount / windowSeconds
            : 0m;

        var averageTransactionSize = command.TransactionCount > 0
            ? volume / command.TransactionCount
            : 0m;

        // Deterministic ObservedAt: derived from window boundary
        var observedAt = command.WindowEnd;

        // Parse IDs
        var analysisId = Guid.Parse(command.AnalysisId);
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

        await _domainService.CreateAnalysisAsync(
            execCtx,
            analysisId, identityId, walletId, command.Scope,
            volume, velocity, command.TransactionCount,
            command.CorrelationId, command.SourceEventId,
            observedAt, command.WindowStart, command.WindowEnd);

        // Build result DTO
        var result = new AnalysisResultDto
        {
            AnalysisId = command.AnalysisId,
            IdentityId = command.IdentityId,
            WalletId = command.WalletId,
            Scope = command.Scope,
            Volume = volume,
            Velocity = velocity,
            TransactionCount = command.TransactionCount,
            AverageTransactionSize = averageTransactionSize,
            WindowStart = command.WindowStart,
            WindowEnd = command.WindowEnd,
            ObservedAt = observedAt,
            CorrelationId = command.CorrelationId,
            SourceEventId = command.SourceEventId
        };

        return EngineResult.Ok(result);
    }

    private static string? Validate(AnalyzeEconomicCommand command)
    {
        if (command.WindowEnd <= command.WindowStart)
            return "WindowEnd must be after WindowStart.";
        if (command.TransactionCount < 0)
            return "TransactionCount cannot be negative.";
        if (command.TotalCredits < 0)
            return "TotalCredits cannot be negative.";
        if (command.TotalDebits < 0)
            return "TotalDebits cannot be negative.";
        if (string.IsNullOrWhiteSpace(command.CorrelationId))
            return "CorrelationId is required.";
        if (string.IsNullOrWhiteSpace(command.SourceEventId))
            return "SourceEventId is required.";
        if (string.IsNullOrWhiteSpace(command.AnalysisId))
            return "AnalysisId is required.";
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return "IdentityId is required.";
        if (string.IsNullOrWhiteSpace(command.Scope))
            return "Scope is required.";
        return null;
    }
}

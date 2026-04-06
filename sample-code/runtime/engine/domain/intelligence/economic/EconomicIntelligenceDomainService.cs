using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.IntelligenceSystem.Economic;
using Whycespace.Domain.IntelligenceSystem.Economic.Analysis;
using Whycespace.Domain.IntelligenceSystem.Economic.Anomaly;
using Whycespace.Domain.IntelligenceSystem.Economic.Forecast;
using Whycespace.Domain.IntelligenceSystem.Economic.Integrity;
using Whycespace.Domain.IntelligenceSystem.Economic.Optimization;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Intelligence.Economic;

/// <summary>
/// Runtime implementation of IEconomicIntelligenceDomainService.
/// Bridges engine layer to domain aggregate creation for economic intelligence.
/// </summary>
public sealed class EconomicIntelligenceDomainService : GovernedDomainServiceBase, IEconomicIntelligenceDomainService
{
    public EconomicIntelligenceDomainService(
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
    }

    public async Task<DomainOperationResult> CreateAnalysisAsync(
        DomainExecutionContext context,
        Guid id, Guid identityId, Guid? walletId, string scope,
        decimal volume, decimal velocity, int transactionCount,
        string correlationId, string sourceEventId,
        DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var analysisScope = new AnalysisScope(scope);
            var analysis = EconomicAnalysisAggregate.Create(
                id, identityId, walletId, analysisScope,
                new Volume(volume), new Velocity(velocity), new TransactionCount(transactionCount),
                correlationId, sourceEventId,
                new ObservationWindow(observedAt, windowStart, windowEnd));

            return (analysis.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateForecastAsync(
        DomainExecutionContext context,
        Guid id, Guid identityId, Guid? walletId, string scope,
        string forecastType, string timeHorizon, decimal predictedValue, decimal confidence,
        string correlationId, string sourceEventId,
        DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var analysisScope = new AnalysisScope(scope);
            var ft = new ForecastType(forecastType);
            var th = new TimeHorizon(timeHorizon);

            var forecast = EconomicForecastAggregate.Create(
                id, identityId, walletId, analysisScope, ft, th, predictedValue,
                new ConfidenceScore(confidence), correlationId, sourceEventId,
                new ObservationWindow(observedAt, windowStart, windowEnd));

            return (forecast.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateAnomalyAsync(
        DomainExecutionContext context,
        Guid id, Guid identityId, Guid? walletId, string scope,
        string severityLevel, decimal confidence, decimal deviation, decimal deviationPercentage,
        string correlationId, string sourceEventId,
        DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var analysisScope = new AnalysisScope(scope);
            var severity = new SeverityLevel(severityLevel);

            var anomaly = EconomicAnomalyAggregate.Create(
                id, identityId, walletId, analysisScope,
                deviation, deviationPercentage, severity,
                new ConfidenceScore(confidence), correlationId, sourceEventId,
                new ObservationWindow(observedAt, windowStart, windowEnd));

            return (anomaly.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateOptimizationAsync(
        DomainExecutionContext context,
        Guid id, Guid identityId, Guid? walletId, string scope,
        string recommendationType, string recommendationAction, decimal impactEstimate, decimal confidence,
        string correlationId, string sourceEventId,
        DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var analysisScope = new AnalysisScope(scope);
            var recType = new RecommendationType(recommendationType);

            var optimization = EconomicOptimizationAggregate.Create(
                id, identityId, walletId, analysisScope,
                recType, recommendationAction,
                new ImpactEstimate(impactEstimate),
                new ConfidenceScore(confidence), correlationId, sourceEventId,
                new ObservationWindow(observedAt, windowStart, windowEnd));

            return (optimization.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateIntegrityCheckAsync(
        DomainExecutionContext context,
        Guid id, Guid identityId, Guid? walletId, string scope,
        decimal integrityScore, decimal calibratedConfidence, bool conflictDetected, string? conflictReason,
        string correlationId,
        DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var analysisScope = new AnalysisScope(scope);

            var integrity = IntelligenceIntegrityAggregate.Create(
                id, identityId, walletId, analysisScope,
                integrityScore, calibratedConfidence,
                conflictDetected, conflictReason,
                correlationId,
                new ObservationWindow(observedAt, windowStart, windowEnd));

            return (integrity.Id, (object?)null);
        });
    }
}

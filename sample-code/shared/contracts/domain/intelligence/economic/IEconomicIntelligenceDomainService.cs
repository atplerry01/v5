using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Intelligence.Economic;

public interface IEconomicIntelligenceDomainService
{
    Task<DomainOperationResult> CreateAnalysisAsync(DomainExecutionContext context, Guid id, Guid identityId, Guid? walletId, string scope, decimal volume, decimal velocity, int transactionCount, string correlationId, string sourceEventId, DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd);
    Task<DomainOperationResult> CreateForecastAsync(DomainExecutionContext context, Guid id, Guid identityId, Guid? walletId, string scope, string forecastType, string timeHorizon, decimal predictedValue, decimal confidence, string correlationId, string sourceEventId, DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd);
    Task<DomainOperationResult> CreateAnomalyAsync(DomainExecutionContext context, Guid id, Guid identityId, Guid? walletId, string scope, string severityLevel, decimal confidence, decimal deviation, decimal deviationPercentage, string correlationId, string sourceEventId, DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd);
    Task<DomainOperationResult> CreateOptimizationAsync(DomainExecutionContext context, Guid id, Guid identityId, Guid? walletId, string scope, string recommendationType, string recommendationAction, decimal impactEstimate, decimal confidence, string correlationId, string sourceEventId, DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd);
    Task<DomainOperationResult> CreateIntegrityCheckAsync(DomainExecutionContext context, Guid id, Guid identityId, Guid? walletId, string scope, decimal integrityScore, decimal calibratedConfidence, bool conflictDetected, string? conflictReason, string correlationId, DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd);
}

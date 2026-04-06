namespace Whycespace.Domain.IntelligenceSystem.Economic.Anomaly;

public sealed class AnomalyDetectionSpec : Specification<EconomicAnomalyAggregate>
{
    public override bool IsSatisfiedBy(EconomicAnomalyAggregate entity) =>
        entity.IdentityId != Guid.Empty
        && entity.Severity is not null
        && entity.Deviation >= 0
        && entity.DeviationPercentage >= 0
        && !string.IsNullOrWhiteSpace(entity.CorrelationId)
        && !string.IsNullOrWhiteSpace(entity.SourceEventId)
        && entity.Window is not null
        && entity.Window.WindowStart <= entity.Window.WindowEnd
        && entity.ConfidenceScore.Value >= 0
        && entity.ConfidenceScore.Value <= 1
        && entity.Scope is not null;
}

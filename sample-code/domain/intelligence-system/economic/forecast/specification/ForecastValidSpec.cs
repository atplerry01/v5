namespace Whycespace.Domain.IntelligenceSystem.Economic.Forecast;

public sealed class ForecastValidSpec : Specification<EconomicForecastAggregate>
{
    public override bool IsSatisfiedBy(EconomicForecastAggregate entity) =>
        entity.IdentityId != Guid.Empty
        && entity.ForecastType is not null
        && entity.TimeHorizon is not null
        && !string.IsNullOrWhiteSpace(entity.CorrelationId)
        && !string.IsNullOrWhiteSpace(entity.SourceEventId)
        && entity.Window is not null
        && entity.Window.WindowStart <= entity.Window.WindowEnd
        && entity.ConfidenceScore.Value >= 0
        && entity.ConfidenceScore.Value <= 1
        && entity.Scope is not null;
}

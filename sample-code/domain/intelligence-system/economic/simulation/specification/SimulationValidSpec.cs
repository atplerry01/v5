namespace Whycespace.Domain.IntelligenceSystem.Economic.Simulation;

public sealed class SimulationValidSpec : Specification<EconomicSimulationAggregate>
{
    public override bool IsSatisfiedBy(EconomicSimulationAggregate entity) =>
        entity.ScenarioType is not null
        && entity.InputParameters.Count > 0
        && !string.IsNullOrWhiteSpace(entity.CorrelationId)
        && !string.IsNullOrWhiteSpace(entity.SourceEventId)
        && entity.Window is not null
        && entity.Window.WindowStart <= entity.Window.WindowEnd;
}

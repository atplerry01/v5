namespace Whycespace.Domain.IntelligenceSystem.Economic.Simulation;

public sealed record EconomicSimulationExecutedEvent(
    Guid SimulationId,
    string ScenarioType
) : DomainEvent;

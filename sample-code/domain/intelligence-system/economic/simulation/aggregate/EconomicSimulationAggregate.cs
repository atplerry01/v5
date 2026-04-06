namespace Whycespace.Domain.IntelligenceSystem.Economic.Simulation;

public sealed class EconomicSimulationAggregate : AggregateRoot
{
    public ScenarioType ScenarioType { get; private set; } = default!;
    public IReadOnlyDictionary<string, decimal> InputParameters => _inputParameters;
    public SimulationResult? ResultSummary { get; private set; }
    public EconomicIntelligenceStatus Status { get; private set; } = EconomicIntelligenceStatus.Pending;

    // Traceability
    public string CorrelationId { get; private set; } = string.Empty;
    public string SourceEventId { get; private set; } = string.Empty;

    // Time context
    public ObservationWindow? Window { get; private set; }

    private readonly Dictionary<string, decimal> _inputParameters = new();

    private EconomicSimulationAggregate() { }

    public static EconomicSimulationAggregate Create(
        Guid simulationId,
        ScenarioType scenarioType,
        Dictionary<string, decimal> inputParameters)
    {
        Guard.AgainstDefault(simulationId);
        Guard.AgainstNull(scenarioType);
        Guard.AgainstNull(inputParameters);

        var simulation = new EconomicSimulationAggregate();
        simulation.Apply(new EconomicSimulationExecutedEvent(
            simulationId, scenarioType.Value));
        foreach (var kvp in inputParameters)
            simulation._inputParameters[kvp.Key] = kvp.Value;
        return simulation;
    }

    public void SetTraceability(string correlationId, string sourceEventId)
    {
        Guard.AgainstEmpty(correlationId);
        Guard.AgainstEmpty(sourceEventId);
        CorrelationId = correlationId;
        SourceEventId = sourceEventId;
    }

    public void SetObservationWindow(ObservationWindow window)
    {
        Guard.AgainstNull(window);
        Window = window;
    }

    public void RecordResult(SimulationResult result)
    {
        Guard.AgainstNull(result);
        EnsureNotTerminal(Status, s => s.IsTerminal, "RecordResult");

        ResultSummary = result;
        Status = EconomicIntelligenceStatus.Completed;
    }

    public void MarkFailed()
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "MarkFailed");
        Status = EconomicIntelligenceStatus.Failed;
    }

    private void Apply(EconomicSimulationExecutedEvent e)
    {
        Id = e.SimulationId;
        ScenarioType = new ScenarioType(e.ScenarioType);
        RaiseDomainEvent(e);
    }
}

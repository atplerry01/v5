namespace Whycespace.Engines.T3I.Forecasting;

public sealed class ScenarioEngine
{
    public ScenarioResult Simulate(ScenarioCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new ScenarioResult(command.ScenarioId, true, []);
    }
}

public sealed record ScenarioCommand(string ScenarioId, string Description, IReadOnlyList<string> Parameters);

public sealed record ScenarioResult(string ScenarioId, bool Success, IReadOnlyList<string> Outcomes);

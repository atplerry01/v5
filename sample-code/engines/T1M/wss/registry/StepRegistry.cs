namespace Whycespace.Engines.T1M.Wss.Registry;

/// <summary>
/// Registry mapping step names to their target engine identifiers.
/// Stateless — stores step-to-engine mappings, no domain logic.
/// </summary>
public sealed class StepRegistry
{
    private readonly Dictionary<string, string> _stepEngineMap = new();

    public void Register(string stepName, string engineTarget)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        ArgumentException.ThrowIfNullOrWhiteSpace(engineTarget);
        _stepEngineMap[stepName] = engineTarget;
    }

    public string ResolveEngine(string stepName)
    {
        if (!_stepEngineMap.TryGetValue(stepName, out var engineTarget))
            throw new InvalidOperationException($"Step '{stepName}' not registered in StepRegistry.");

        return engineTarget;
    }

    public bool HasStep(string stepName) => _stepEngineMap.ContainsKey(stepName);
}

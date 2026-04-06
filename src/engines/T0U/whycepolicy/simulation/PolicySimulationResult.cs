namespace Whyce.Engines.T0U.WhycePolicy.Simulation;

/// <summary>
/// Result of a policy simulation (dry-run evaluation).
/// Used for impact assessment without actual enforcement.
/// </summary>
public sealed record PolicySimulationResult(
    bool WouldBeCompliant,
    string[] RulesEvaluated,
    string SimulationHash,
    string? PredictedDenialReason);

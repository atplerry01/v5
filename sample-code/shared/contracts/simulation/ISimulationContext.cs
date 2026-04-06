namespace Whycespace.Shared.Contracts.Simulation;

/// <summary>
/// Unified simulation context for predictive governance (E8).
/// Carries all dimensions needed for cross-domain simulation:
/// identity, policy, economic, and workflow.
/// </summary>
public interface ISimulationContext
{
    string SimulationId { get; }
    string ScenarioType { get; }
    string SubjectId { get; }
    string Action { get; }
    string Resource { get; }
    decimal? Amount { get; }
    string? Currency { get; }
    string? WorkflowId { get; }
    string? AccountId { get; }
    double TrustScore { get; }
    string[] Roles { get; }
}

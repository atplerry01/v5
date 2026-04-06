using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.PolicySimulation.Scenario;

/// <summary>
/// Constructs simulation scenarios from targets and context.
/// Stateless — resolves policy versions from IPolicyReadModel (projections).
/// Read-only — does not mutate policy state.
/// </summary>
public sealed class PolicyScenarioBuilder
{
    private readonly IPolicyReadModel _readModel;
    private readonly IClock _clock;

    public PolicyScenarioBuilder(IPolicyReadModel readModel, IClock clock)
    {
        _readModel = readModel ?? throw new ArgumentNullException(nameof(readModel));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<SimulationScenario> BuildAsync(
        PolicySimulationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var resolvedVersions = new List<ResolvedSimulationPolicy>();

        foreach (var target in command.Targets)
        {
            var version = target.Version.HasValue
                ? await _readModel.GetVersionAsync(target.PolicyId, target.Version.Value, cancellationToken)
                : await _readModel.GetActiveVersionAsync(target.PolicyId, cancellationToken);

            if (version is null)
            {
                resolvedVersions.Add(new ResolvedSimulationPolicy(
                    target.PolicyId, null, target.IsOverride, false, "Version not found"));
                continue;
            }

            resolvedVersions.Add(new ResolvedSimulationPolicy(
                target.PolicyId, version, target.IsOverride, true, null));
        }

        return new SimulationScenario(
            command.ScenarioId,
            resolvedVersions.AsReadOnly(),
            command.Context,
            command.SimulatedTime ?? _clock.UtcNowOffset);
    }

    /// <summary>
    /// Extends an existing scenario with federation graph metadata.
    /// Returns a new scenario instance with federation context attached.
    /// Read-only — does not mutate the original scenario.
    /// </summary>
    public SimulationScenario WithFederationGraph(
        SimulationScenario scenario,
        FederationGraphDto federationGraph)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(federationGraph);

        return scenario with
        {
            FederationGraph = new FederationGraphContext(
                federationGraph.FederationId,
                federationGraph.GraphHash,
                federationGraph.Nodes.Count,
                federationGraph.Edges.Count,
                federationGraph.Nodes
                    .Select(n => n.ClusterId)
                    .Distinct()
                    .ToList()
                    .AsReadOnly())
        };
    }
}

public sealed record SimulationScenario(
    Guid ScenarioId,
    IReadOnlyList<ResolvedSimulationPolicy> Policies,
    SimulationContext Context,
    DateTimeOffset SimulatedTime)
{
    /// <summary>Federation graph context when simulating cross-cluster policies. Null if non-federated.</summary>
    public FederationGraphContext? FederationGraph { get; init; }
}

public sealed record ResolvedSimulationPolicy(
    Guid PolicyId,
    PolicyVersionRecord? Version,
    bool IsOverride,
    bool IsResolved,
    string? Error);

/// <summary>
/// Immutable federation graph context attached to a simulation scenario.
/// Contains graph metadata for cross-cluster policy evaluation.
/// </summary>
public sealed record FederationGraphContext(
    Guid FederationId,
    string GraphHash,
    int NodeCount,
    int EdgeCount,
    IReadOnlyList<string> ClusterIds);

using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Simulation;

/// <summary>
/// Runtime adapter that reuses the policy evaluation pipeline in simulation mode.
/// Delegates to IPolicyEngineInvoker with PolicyExecutionMode.Simulation.
/// NEVER triggers enforcement — simulation only.
/// </summary>
public sealed class PolicySimulationAdapter
{
    private readonly IPolicyEngineInvoker _policyInvoker;

    public PolicySimulationAdapter(IPolicyEngineInvoker policyInvoker)
    {
        _policyInvoker = policyInvoker ?? throw new ArgumentNullException(nameof(policyInvoker));
    }

    /// <summary>
    /// Simulates a policy evaluation. Returns the result without enforcement.
    /// </summary>
    public async Task<PolicyEvaluationResult> SimulateAsync(
        PolicyEvaluationInput input,
        CancellationToken cancellationToken = default)
    {
        // Always run in Simulation mode — never enforce
        return await _policyInvoker.InvokeAsync(input, PolicyExecutionMode.Simulation, cancellationToken);
    }
}

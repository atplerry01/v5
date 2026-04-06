using Whycespace.Engines.T0U.WhycePolicy.Evaluation;
using Whycespace.Engines.T0U.WhycePolicy.Registry;

namespace Whycespace.Engines.T0U.WhycePolicy.Simulation;

public sealed class PolicySimulationHandler : IPolicySimulationEngine
{
    private readonly IPolicyEvaluationEngine _evaluationEngine;

    public PolicySimulationHandler(IPolicyEvaluationEngine evaluationEngine)
    {
        ArgumentNullException.ThrowIfNull(evaluationEngine);
        _evaluationEngine = evaluationEngine;
    }

    public async Task<PolicyEvaluationEngineResult> SimulateAsync(
        PolicyContext context,
        CancellationToken cancellationToken = default)
    {
        return await _evaluationEngine.EvaluateAsync(context, cancellationToken);
    }
}

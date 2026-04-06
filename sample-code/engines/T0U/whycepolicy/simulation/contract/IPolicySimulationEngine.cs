using Whycespace.Engines.T0U.WhycePolicy.Evaluation;
using Whycespace.Engines.T0U.WhycePolicy.Registry;

namespace Whycespace.Engines.T0U.WhycePolicy.Simulation;

public interface IPolicySimulationEngine
{
    Task<PolicyEvaluationEngineResult> SimulateAsync(
        PolicyContext context,
        CancellationToken cancellationToken = default);
}

using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T0U.WhycePolicy.Evaluation;

/// <summary>
/// Abstraction for policy execution backends.
/// Evaluation handler depends on this — NOT on OPA directly.
/// Implementations: OPA, Domain, Simulation.
/// </summary>
public interface IPolicyExecutionProvider
{
    Task<PolicyEvaluationResult> ExecuteAsync(
        PolicyEvaluationInput input,
        CancellationToken cancellationToken = default);
}

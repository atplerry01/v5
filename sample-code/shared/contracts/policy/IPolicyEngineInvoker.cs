namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Engine invocation contract for policy evaluation.
/// Middleware calls this instead of IPolicyEvaluator directly.
/// Implementation bridges to T0U WhycePolicy evaluation engine
/// through the engine adapter pattern.
/// </summary>
public interface IPolicyEngineInvoker
{
    Task<PolicyEvaluationResult> InvokeAsync(
        PolicyEvaluationInput input,
        PolicyExecutionMode mode = PolicyExecutionMode.Enforcement,
        CancellationToken cancellationToken = default);
}

public enum PolicyExecutionMode
{
    Enforcement,
    Simulation
}

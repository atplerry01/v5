using Whycespace.Engines.T0U.WhycePolicy.Evaluation;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T0U.WhycePolicy.Opa;

/// <summary>
/// OPA-backed implementation of IPolicyExecutionProvider.
/// Delegates to OpaEvaluationEngine handler.
/// Lives in opa/adapter — evaluation layer has NO OPA knowledge.
/// </summary>
public sealed class OpaPolicyExecutionProvider : IPolicyExecutionProvider, IPolicyEvaluator, IPolicyEngineInvoker
{
    private readonly OpaEvaluationEngine _handler;

    public OpaPolicyExecutionProvider(OpaEvaluationEngine handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public Task<PolicyEvaluationResult> ExecuteAsync(
        PolicyEvaluationInput input,
        CancellationToken cancellationToken = default)
    {
        return _handler.EvaluateAsync(input, cancellationToken);
    }

    public Task<PolicyEvaluationResult> EvaluateAsync(
        PolicyEvaluationInput input,
        CancellationToken cancellationToken = default)
    {
        return _handler.EvaluateAsync(input, cancellationToken);
    }

    public Task<PolicyEvaluationResult> InvokeAsync(
        PolicyEvaluationInput input,
        PolicyExecutionMode mode = PolicyExecutionMode.Enforcement,
        CancellationToken cancellationToken = default)
    {
        return _handler.EvaluateAsync(input, cancellationToken);
    }
}

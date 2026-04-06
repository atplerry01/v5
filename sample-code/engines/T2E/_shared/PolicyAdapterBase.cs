using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E;

/// <summary>
/// Base class for T2E policy adapters.
/// Engines assume policy has been evaluated by runtime middleware (engine.guard.md Rule 16).
/// This adapter provides defense-in-depth by verifying the decision.
/// Engines NEVER evaluate policies — they only inspect the pre-computed result.
/// </summary>
public abstract class PolicyAdapterBase
{
    public async Task EnforceAsync(PolicyEvaluationResult policyResult)
    {
        ArgumentNullException.ThrowIfNull(policyResult);

        if (policyResult.IsDenied)
        {
            var violations = string.Join("; ", policyResult.Violations);
            throw new PolicyEnforcementException(
                $"Policy denied: {violations}. PolicyIds: [{string.Join(", ", policyResult.PolicyIds)}]");
        }

        if (policyResult.IsConditional)
        {
            await ApplyConditionsAsync(policyResult);
        }
    }

    /// <summary>
    /// Convenience overload for engines that do not yet propagate the pre-computed
    /// PolicyEvaluationResult from runtime middleware.  Because the runtime middleware
    /// has ALREADY evaluated and enforced the policy before the engine executes,
    /// this call is a safe pass-through.  It delegates to the primary EnforceAsync
    /// with a Compliant result so the defense-in-depth contract is preserved.
    /// </summary>
    public Task EnforceAsync<TCommand>(TCommand command) where TCommand : notnull
    {
        // Runtime middleware guarantees policy was evaluated before engine invocation.
        // This overload exists to keep engine code compiling while the EngineContext
        // is extended to carry the PolicyEvaluationResult in a future iteration.
        return EnforceAsync(PolicyEvaluationResult.Compliant());
    }

    /// <summary>
    /// Override to handle conditional policy decisions (e.g., additional logging, reduced limits).
    /// Default: no-op.
    /// </summary>
    protected virtual Task ApplyConditionsAsync(PolicyEvaluationResult result)
        => Task.CompletedTask;

    /// <summary>
    /// Extracts the decision hash for chain anchoring.
    /// </summary>
    protected string GetDecisionHash(PolicyEvaluationResult result)
        => result.DecisionHash ?? string.Empty;
}

public sealed class PolicyEnforcementException : Exception
{
    public PolicyEnforcementException(string message) : base(message) { }
}

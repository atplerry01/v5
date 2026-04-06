using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Coordinates multi-policy evaluation across systems.
/// Aggregates decisions: identity + economic + workflow + operational.
/// Deterministic, priority-aware aggregation.
/// </summary>
public sealed class PolicyOrchestrator
{
    private readonly IPolicyEngineInvoker _invoker;

    public PolicyOrchestrator(IPolicyEngineInvoker invoker)
    {
        _invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
    }

    public async Task<PolicyOrchestratorResult> EvaluateMultipleAsync(
        IReadOnlyList<GlobalPolicyContext> contexts,
        PolicyExecutionMode mode = PolicyExecutionMode.Enforcement,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);

        if (contexts.Count == 0)
            return PolicyOrchestratorResult.Allowed([]);

        var results = new List<PolicyEvaluationResult>();

        foreach (var context in contexts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var input = context.ToEvaluationInput();
            var result = await _invoker.InvokeAsync(input, mode, cancellationToken);
            results.Add(result);
        }

        return AggregateDecisions(results);
    }

    private static PolicyOrchestratorResult AggregateDecisions(List<PolicyEvaluationResult> results)
    {
        // Aggregation rule: any DENY → DENY, any CONDITIONAL → CONDITIONAL, else ALLOW
        if (results.Any(r => r.IsDenied))
        {
            var allViolations = results
                .Where(r => r.IsDenied)
                .SelectMany(r => r.Violations)
                .ToList();

            return PolicyOrchestratorResult.Denied(allViolations, results);
        }

        if (results.Any(r => r.IsConditional))
        {
            var conditions = results
                .Where(r => r.IsConditional)
                .SelectMany(r => r.Violations)
                .ToList();

            return PolicyOrchestratorResult.Conditional(conditions, results);
        }

        return PolicyOrchestratorResult.Allowed(results);
    }
}

public sealed record PolicyOrchestratorResult
{
    public required string AggregatedDecision { get; init; }
    public required bool IsAllowed { get; init; }
    public IReadOnlyList<string> Violations { get; init; } = [];
    public IReadOnlyList<PolicyEvaluationResult> IndividualResults { get; init; } = [];

    public bool IsDenied => !IsAllowed && AggregatedDecision == "DENY";

    public static PolicyOrchestratorResult Allowed(IReadOnlyList<PolicyEvaluationResult> results) => new()
    {
        AggregatedDecision = "ALLOW",
        IsAllowed = true,
        IndividualResults = results
    };

    public static PolicyOrchestratorResult Denied(
        IReadOnlyList<string> violations,
        IReadOnlyList<PolicyEvaluationResult> results) => new()
    {
        AggregatedDecision = "DENY",
        IsAllowed = false,
        Violations = violations,
        IndividualResults = results
    };

    public static PolicyOrchestratorResult Conditional(
        IReadOnlyList<string> conditions,
        IReadOnlyList<PolicyEvaluationResult> results) => new()
    {
        AggregatedDecision = "CONDITIONAL",
        IsAllowed = true,
        Violations = conditions,
        IndividualResults = results
    };
}

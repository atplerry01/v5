using Whycespace.Domain.IntelligenceSystem.Economic.Autonomy;
using Whycespace.Runtime.Economic.Autonomy.Result;
using Whycespace.Shared.Contracts.Domain.Economic;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Economic.Autonomy;

/// <summary>
/// Policy-bounded autonomous decision orchestrator (E18.5).
/// Selects execution paths automatically ONLY when:
/// 1. Autonomy is enabled via context
/// 2. WHYCEPOLICY approves the action
/// 3. Decision is within constraints
///
/// CRITICAL: This is a CONTROLLED CAPABILITY — does NOT self-activate.
/// Falls back to manual/orchestrated execution when any gate fails.
/// All decisions are deterministic and chain-anchor-ready.
/// </summary>
public sealed class AutonomyOrchestrator
{
    private readonly AutonomousDecisionService _decisionService;
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IClock _clock;

    public AutonomyOrchestrator(
        AutonomousDecisionService decisionService,
        IPolicyEvaluator policyEvaluator,
        IClock clock)
    {
        _decisionService = decisionService;
        _policyEvaluator = policyEvaluator;
        _clock = clock;
    }

    public async Task<AutonomyExecutionResult> ExecuteAsync(
        IEnumerable<AutonomousCandidate> candidates,
        AutonomyConstraints constraints,
        IAutonomyContext context,
        Guid actorId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Gate 1: Autonomy must be explicitly enabled
        if (!context.IsAutonomyEnabled)
            return AutonomyExecutionResult.Disabled();

        // Gate 2: Policy evaluation
        var policyInput = new PolicyEvaluationInput(
            PolicyId: null,
            ActorId: actorId,
            Action: "autonomy.execute",
            Resource: "economic.execution-path",
            Environment: context.Environment,
            Timestamp: _clock.UtcNowOffset);

        var policyResult = await _policyEvaluator.EvaluateAsync(policyInput, ct);

        if (!policyResult.IsCompliant)
            return AutonomyExecutionResult.Blocked(
                policyResult.Violation ?? "Policy denied autonomous execution");

        // Gate 3: Domain decision within constraints
        var decision = _decisionService.Decide(candidates, constraints);

        if (decision is null)
            return AutonomyExecutionResult.NoCandidates();

        return AutonomyExecutionResult.Success(decision);
    }
}

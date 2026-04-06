using Whycespace.Runtime.Activation.Result;
using Whycespace.Shared.Contracts.Domain.Operational;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Activation;

/// <summary>
/// System activation orchestrator (EX).
/// Controls execution mode transitions through policy-gated validation.
///
/// Modes: SIMULATION → SANDBOX → LIMITED_PRODUCTION → FULL_PRODUCTION
///
/// CRITICAL: No production activation without policy approval.
/// Simulation mode bypasses real execution while running the full pipeline.
/// </summary>
public sealed class SystemActivationOrchestrator
{
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IClock _clock;

    public SystemActivationOrchestrator(
        IPolicyEvaluator policyEvaluator,
        IClock clock)
    {
        _policyEvaluator = policyEvaluator;
        _clock = clock;
    }

    public async Task<ActivationResult> ExecuteAsync(
        IActivationContext context,
        Guid actorId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Simulation mode runs without policy gate (dry-run)
        if (context.IsSimulation)
            return ActivationResult.Simulation();

        // All non-simulation modes require policy approval
        var policyInput = new PolicyEvaluationInput(
            PolicyId: null,
            ActorId: actorId,
            Action: $"system.activate.{context.Mode.ToLowerInvariant()}",
            Resource: "system.activation",
            Environment: context.Mode,
            Timestamp: _clock.UtcNowOffset);

        var policyResult = await _policyEvaluator.EvaluateAsync(policyInput, ct);

        if (!policyResult.IsCompliant)
            return ActivationResult.Blocked(
                policyResult.Violation ?? "Policy denied activation");

        return ActivationResult.Active(context.Mode);
    }
}

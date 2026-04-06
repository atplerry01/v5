using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// Runtime middleware — controls how federated trust propagates to identity trust (E9).
///
/// FIX 4: Injects propagation output into:
///   - Identity intelligence context (E9) via EffectiveFederatedTrust
///   - Policy input via PropagationStatus
///
/// Pipeline position: AFTER T3I computation, BEFORE final response.
/// NO engine logic — delegates to injected guard only.
/// </summary>
public sealed class FederationPropagationMiddleware : IMiddleware
{
    private readonly IFederationPropagationGuard _guard;

    public FederationPropagationMiddleware(IFederationPropagationGuard guard)
    {
        _guard = guard ?? throw new ArgumentNullException(nameof(guard));
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var result = await next(context);

        if (!result.Success)
            return result;

        var input = context.Get<PropagationInput>(ContextKeys.PropagationInput);
        if (input is null)
            return result;

        var guardResult = _guard.Evaluate(input);
        context.Set(ContextKeys.PropagationResult, guardResult);

        if (!guardResult.Allowed)
        {
            context.Set(ContextKeys.PropagationBlocked, true);
        }

        // FIX 4: Inject propagation output for E9 intelligence + policy consumers
        var propagationOutput = new FederationPropagationOutput
        {
            EffectiveFederatedTrust = guardResult.Allowed ? guardResult.CappedContribution : 0m,
            PropagationStatus = guardResult.Allowed ? "Propagated" : "Blocked",
            BlockReason = guardResult.BlockReason
        };
        context.Set(ContextKeys.PropagationOutput, propagationOutput);

        // FIX 5: Emit observability signal
        context.Set(FederationObservability.Keys.PropagationResult, new FederationObservability.PropagationSignal(
            propagationOutput.PropagationStatus,
            propagationOutput.EffectiveFederatedTrust,
            propagationOutput.BlockReason));

        return result;
    }

    public static class ContextKeys
    {
        public const string PropagationInput = "Federation.PropagationInput";
        public const string PropagationResult = "Federation.PropagationResult";
        public const string PropagationBlocked = "Federation.PropagationBlocked";
        public const string PropagationOutput = "Federation.PropagationOutput";
    }
}

/// <summary>
/// FIX 4: Propagation output injected into identity intelligence context and policy input.
/// </summary>
public sealed record FederationPropagationOutput
{
    public required decimal EffectiveFederatedTrust { get; init; }
    public required string PropagationStatus { get; init; }
    public string? BlockReason { get; init; }
}

/// <summary>
/// Runtime-layer abstraction for trust propagation guard.
/// Implemented by T3I FederationTrustPropagationGuard at composition root.
/// </summary>
public interface IFederationPropagationGuard
{
    PropagationResult Evaluate(PropagationInput input);
}

public sealed record PropagationInput
{
    public required decimal NormalizedTrustScore { get; init; }
    public required decimal LinkConfidence { get; init; }
    public required string IssuerReputationStatus { get; init; }
    public required string IssuerTrustStatus { get; init; }
    public required decimal MaxTrustContribution { get; init; }
    public required decimal MinConfidenceForPropagation { get; init; }
}

public sealed record PropagationResult
{
    public required bool Allowed { get; init; }
    public required decimal CappedContribution { get; init; }
    public string? BlockReason { get; init; }
}

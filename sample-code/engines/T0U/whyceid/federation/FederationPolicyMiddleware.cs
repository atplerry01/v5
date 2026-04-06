using Whycespace.Engines.T0U.WhyceId.Federation;
using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// Runtime middleware — enforces federation policy before execution.
///
/// FIX 1: Merges WHYCEPOLICY decision with FederationPolicyEnforcer decision.
///   WHYCEPOLICY OVERRIDES ALL.
///
/// Pipeline position: AFTER validation, BEFORE conditional handler.
/// NO engine logic — delegates to T0U engine + FinalPolicyDecisionResolver.
/// Uses string-based types instead of domain enums.
/// </summary>
public sealed class FederationPolicyMiddleware : IMiddleware
{
    private readonly FederationPolicyEnforcer _policyEnforcer;

    public FederationPolicyMiddleware(FederationPolicyEnforcer policyEnforcer)
    {
        _policyEnforcer = policyEnforcer ?? throw new ArgumentNullException(nameof(policyEnforcer));
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var policyInput = context.Get<FederationPolicyInput>(ContextKeys.PolicyInput);
        if (policyInput is null)
            return await next(context);

        // Step 1: Get federation enforcer decision
        var federationDecision = _policyEnforcer.Enforce(new EnforceFederationPolicyCommand(
            IssuerId: policyInput.IssuerId,
            TrustLevel: policyInput.TrustLevel,
            CurrentConfidence: policyInput.CurrentConfidence,
            Provenance: policyInput.Provenance,
            TrustStatus: policyInput.TrustStatus,
            Thresholds: policyInput.Thresholds));

        // Step 2: Merge with WHYCEPOLICY (if present in context)
        var whycePolicyOutcome = context.Get<string>(ContextKeys.WhycePolicyOutcome);
        var whycePolicyReason = context.Get<string>(ContextKeys.WhycePolicyReason);

        var finalDecision = FinalPolicyDecisionResolver.Resolve(
            whycePolicyOutcome, whycePolicyReason, federationDecision);

        context.Set(ContextKeys.PolicyDecision, federationDecision);
        context.Set(ContextKeys.FinalDecision, finalDecision);

        // Step 3: Emit observability signal
        context.Set(FederationObservability.Keys.PolicyDecision, new FederationObservability.PolicyDecisionSignal(
            finalDecision.Outcome, finalDecision.Source, finalDecision.Reason));

        // Step 4: Enforce final decision
        if (finalDecision.IsDenied)
        {
            return CommandResult.Fail(
                context.Envelope.CommandId,
                $"Federation policy denied: {finalDecision.Reason}",
                "FEDERATION_POLICY_DENIED",
                context.Clock.UtcNowOffset);
        }

        if (finalDecision.IsConditional)
        {
            context.Set(ContextKeys.ConditionalFlow, true);
        }

        return await next(context);
    }

    public static class ContextKeys
    {
        public const string PolicyInput = "Federation.PolicyInput";
        public const string PolicyDecision = "Federation.PolicyDecision";
        public const string FinalDecision = "Federation.FinalDecision";
        public const string ConditionalFlow = "Federation.ConditionalFlow";
        public const string WhycePolicyOutcome = "Federation.WhycePolicy.Outcome";
        public const string WhycePolicyReason = "Federation.WhycePolicy.Reason";
    }
}

/// <summary>
/// Policy input for federation middleware — assembled by runtime orchestration.
/// Uses string-based types instead of domain enums.
/// </summary>
public sealed record FederationPolicyInput
{
    public required Guid IssuerId { get; init; }
    public required decimal TrustLevel { get; init; }
    public required decimal CurrentConfidence { get; init; }
    public required string Provenance { get; init; }
    public required string TrustStatus { get; init; }
    public required FederationPolicyThresholds Thresholds { get; init; }
}

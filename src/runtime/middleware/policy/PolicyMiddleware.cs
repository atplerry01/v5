using Whyce.Engines.T0U.WhyceId.Command;
using Whyce.Engines.T0U.WhyceId.Engine;
using Whyce.Engines.T0U.WhycePolicy.Command;
using Whyce.Engines.T0U.WhycePolicy.Engine;
using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware.Policy;

/// <summary>
/// WHYCEPOLICY enforcement middleware. Every command MUST pass through policy evaluation.
/// Policy deny = HARD STOP. No engine execution without explicit policy approval.
///
/// Mandatory Flow (T0U Constitutional):
/// 1. WhyceId → resolve identity (AuthenticateIdentity)
/// 2. WhycePolicy → evaluate compliance (Evaluate)
/// 3. Inject identity + policy decision into context
/// 4. Deny → halt, Allow → proceed
///
/// Non-bypassable: No request without WhyceId. No execution without WhycePolicy.
/// </summary>
public sealed class PolicyMiddleware : IMiddleware
{
    private readonly WhyceIdEngine _whyceIdEngine;
    private readonly WhycePolicyEngine _whycePolicyEngine;

    public PolicyMiddleware(WhyceIdEngine whyceIdEngine, WhycePolicyEngine whycePolicyEngine)
    {
        _whyceIdEngine = whyceIdEngine;
        _whycePolicyEngine = whycePolicyEngine;
    }

    public async Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next)
    {
        // Step 1: Resolve identity via WhyceIdEngine (T0U)
        var authenticateCommand = new AuthenticateIdentityCommand(
            Token: null,
            UserId: context.ActorId,
            DeviceId: null);

        var authResult = await _whyceIdEngine.AuthenticateIdentity(authenticateCommand);

        if (!authResult.IsAuthenticated)
        {
            return CommandResult.Failure(
                "Identity resolution failed. Policy enforcement requires valid identity. No bypass allowed.");
        }

        // Step 2: Inject identity into ExecutionContext
        context.IdentityId = authResult.Identity.IdentityId;
        context.Roles = authResult.Identity.Roles;
        context.TrustScore = authResult.Identity.TrustScore;

        // Step 3: Evaluate policy via WhycePolicyEngine (T0U)
        var evaluateCommand = new EvaluatePolicyCommand(
            PolicyName: context.PolicyId,
            IdentityId: authResult.Identity.IdentityId,
            Roles: authResult.Identity.Roles,
            TrustScore: authResult.Identity.TrustScore,
            CommandType: command.GetType().Name,
            TenantId: context.TenantId,
            ResourceId: null);

        var policyResult = await _whycePolicyEngine.Evaluate(evaluateCommand);

        // Step 4: Inject PolicyDecision into ExecutionContext (write-once — locked after this)
        context.PolicyDecisionAllowed = policyResult.IsCompliant;
        context.PolicyDecisionHash = policyResult.DecisionHash;
        context.PolicyVersion = policyResult.PolicyVersion;

        // Step 5: Policy deny = HARD STOP
        if (!policyResult.IsCompliant)
        {
            return CommandResult.Failure(
                $"WHYCEPOLICY denied: {policyResult.DenialReason ?? "execution not permitted"}. No bypass allowed.");
        }

        return await next();
    }
}

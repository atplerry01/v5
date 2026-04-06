using Whyce.Engines.T0U.WhyceId;
using Whyce.Engines.T0U.WhycePolicy;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Pipeline;

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
        // Step 1: Resolve identity via WhyceIdEngine
        var idCommand = new WhyceIdCommand(Token: null, UserId: context.ActorId);
        var engineContext = new EngineContext(idCommand, context.AggregateId, (_, _) => Task.FromResult<object>(null!));

        var idResult = await _whyceIdEngine.Handle(idCommand, engineContext);

        if (!idResult.IsValid)
        {
            return CommandResult.Failure("Identity resolution failed.");
        }

        // Step 2: Inject identity into ExecutionContext
        context.IdentityId = idResult.Identity.IdentityId;
        context.Roles = idResult.Identity.Roles;
        context.TrustScore = idResult.Identity.TrustScore;

        // Step 3: Evaluate policy via WhycePolicyEngine
        var policyCommand = new WhycePolicyCommand(
            PolicyName: context.PolicyId,
            IdentityId: idResult.Identity.IdentityId,
            Roles: idResult.Identity.Roles,
            TrustScore: idResult.Identity.TrustScore);

        var policyDecision = await _whycePolicyEngine.Handle(policyCommand, engineContext);

        // Step 4: Inject PolicyDecision into ExecutionContext
        context.PolicyDecisionAllowed = policyDecision.IsAllowed;
        context.PolicyDecisionHash = policyDecision.DecisionHash;

        if (!policyDecision.IsAllowed)
        {
            return CommandResult.Failure("Policy denied: execution not permitted.");
        }

        return await next();
    }
}

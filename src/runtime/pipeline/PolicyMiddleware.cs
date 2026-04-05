using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Pipeline;

public sealed class PolicyMiddleware : IMiddleware
{
    private readonly IPolicyEvaluator _policyEvaluator;

    public PolicyMiddleware(IPolicyEvaluator policyEvaluator)
    {
        _policyEvaluator = policyEvaluator;
    }

    public async Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next)
    {
        var policyContext = new PolicyContext(
            context.CorrelationId,
            context.TenantId,
            context.ActorId,
            command.GetType().Name);

        var decision = await _policyEvaluator.EvaluateAsync(context.PolicyId, command, policyContext);

        if (!decision.IsAllowed)
        {
            return CommandResult.Failure($"Policy denied: {decision.DenialReason}");
        }

        // Propagate decision hash for chain anchoring
        context = context with { PolicyDecisionHash = decision.DecisionHash };

        return await next();
    }
}

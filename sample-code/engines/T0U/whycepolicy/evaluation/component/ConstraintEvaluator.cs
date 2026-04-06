using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;
using Whycespace.Engines.T0U.WhycePolicy.Registry;

namespace Whycespace.Engines.T0U.WhycePolicy.Evaluation;

public static class ConstraintEvaluator
{
    public static async Task<bool> EvaluateConstraintAsync(
        IPolicyEvaluationDomainService domainService,
        DomainExecutionContext execCtx,
        string expression,
        IReadOnlyDictionary<string, object> facts)
    {
        ArgumentNullException.ThrowIfNull(domainService);
        ArgumentNullException.ThrowIfNull(execCtx);
        ArgumentNullException.ThrowIfNull(facts);

        var result = await domainService.EvaluateConstraintAsync(execCtx, expression, facts);
        return result.Passed;
    }

    public static IReadOnlyDictionary<string, object> BuildFacts(PolicyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new Dictionary<string, object>
        {
            ["actorId"] = context.ActorId.ToString(),
            ["action"] = context.Action,
            ["resource"] = context.Resource,
            ["environment"] = context.Environment,
            ["timestamp"] = context.Timestamp.ToString("O")
        };
    }
}

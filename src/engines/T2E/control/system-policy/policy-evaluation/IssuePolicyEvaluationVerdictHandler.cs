using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyEvaluation;

public sealed class IssuePolicyEvaluationVerdictHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not IssuePolicyEvaluationVerdictCommand cmd)
            return;

        var aggregate = (PolicyEvaluationAggregate)await context.LoadAggregateAsync(typeof(PolicyEvaluationAggregate));
        aggregate.IssueVerdict(Enum.Parse<EvaluationOutcome>(cmd.Outcome, ignoreCase: true), cmd.DecisionHash);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

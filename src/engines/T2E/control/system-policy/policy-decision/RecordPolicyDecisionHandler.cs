using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyDecision;

public sealed class RecordPolicyDecisionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordPolicyDecisionCommand cmd)
            return Task.CompletedTask;

        var aggregate = PolicyDecisionAggregate.Record(
            new PolicyDecisionId(cmd.DecisionId.ToString("N").PadRight(64, '0')),
            cmd.PolicyDefinitionId,
            cmd.SubjectId,
            cmd.Action,
            cmd.Resource,
            Enum.Parse<PolicyDecisionOutcome>(cmd.Outcome, ignoreCase: true),
            cmd.DecidedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

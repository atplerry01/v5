using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyEvaluation;

public sealed class RecordPolicyEvaluationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordPolicyEvaluationCommand cmd)
            return Task.CompletedTask;

        var aggregate = PolicyEvaluationAggregate.Record(
            new PolicyEvaluationId(cmd.EvaluationId.ToString("N").PadRight(64, '0')),
            cmd.PolicyId,
            cmd.ActorId,
            cmd.Action,
            cmd.CorrelationId);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

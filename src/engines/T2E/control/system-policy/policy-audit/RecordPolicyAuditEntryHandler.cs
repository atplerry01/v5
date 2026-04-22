using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyAudit;

public sealed class RecordPolicyAuditEntryHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordPolicyAuditEntryCommand cmd)
            return Task.CompletedTask;

        var aggregate = PolicyAuditAggregate.Record(
            new PolicyAuditId(cmd.AuditId.ToString("N").PadRight(64, '0')),
            cmd.PolicyId,
            cmd.ActorId,
            cmd.Action,
            Enum.Parse<PolicyAuditCategory>(cmd.Category, ignoreCase: true),
            cmd.DecisionHash,
            cmd.CorrelationId,
            cmd.OccurredAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

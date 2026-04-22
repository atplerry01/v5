using Whycespace.Domain.ControlSystem.Audit.AuditLog;
using Whycespace.Shared.Contracts.Control.Audit.AuditLog;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditLog;

public sealed class RecordAuditLogEntryHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordAuditLogEntryCommand cmd)
            return Task.CompletedTask;

        var aggregate = AuditLogAggregate.Record(
            new AuditLogId(cmd.AuditLogId.ToString("N").PadRight(64, '0')),
            cmd.ActorId,
            cmd.Action,
            cmd.Resource,
            Enum.Parse<AuditEntryClassification>(cmd.Classification, ignoreCase: true),
            cmd.OccurredAt,
            cmd.DecisionId);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

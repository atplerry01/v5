using Whycespace.Domain.ControlSystem.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditRecord;

public sealed class RaiseAuditRecordHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RaiseAuditRecordCommand cmd)
            return Task.CompletedTask;

        var aggregate = AuditRecordAggregate.Raise(
            new AuditRecordId(cmd.RecordId.ToString("N").PadRight(64, '0')),
            cmd.AuditLogEntryIds,
            cmd.Description,
            Enum.Parse<AuditRecordSeverity>(cmd.Severity, ignoreCase: true),
            cmd.RaisedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

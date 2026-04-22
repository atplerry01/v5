using Whycespace.Domain.ControlSystem.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditEvent;

public sealed class SealAuditEventHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SealAuditEventCommand cmd)
            return;

        var aggregate = (AuditEventAggregate)await context.LoadAggregateAsync(typeof(AuditEventAggregate));
        aggregate.Seal(cmd.IntegrityHash);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

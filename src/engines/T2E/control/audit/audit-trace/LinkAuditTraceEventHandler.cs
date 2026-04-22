using Whycespace.Domain.ControlSystem.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditTrace;

public sealed class LinkAuditTraceEventHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not LinkAuditTraceEventCommand cmd)
            return;

        var aggregate = (AuditTraceAggregate)await context.LoadAggregateAsync(typeof(AuditTraceAggregate));
        aggregate.LinkEvent(cmd.AuditEventId);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

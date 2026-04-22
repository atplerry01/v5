using Whycespace.Domain.ControlSystem.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditTrace;

public sealed class CloseAuditTraceHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CloseAuditTraceCommand cmd)
            return;

        var aggregate = (AuditTraceAggregate)await context.LoadAggregateAsync(typeof(AuditTraceAggregate));
        aggregate.Close(cmd.ClosedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

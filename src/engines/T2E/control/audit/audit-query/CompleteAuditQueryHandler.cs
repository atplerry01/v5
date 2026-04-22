using Whycespace.Domain.ControlSystem.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditQuery;

public sealed class CompleteAuditQueryHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteAuditQueryCommand cmd)
            return;

        var aggregate = (AuditQueryAggregate)await context.LoadAggregateAsync(typeof(AuditQueryAggregate));
        aggregate.Complete(cmd.ResultCount);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

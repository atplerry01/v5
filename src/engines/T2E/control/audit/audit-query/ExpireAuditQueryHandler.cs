using Whycespace.Domain.ControlSystem.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditQuery;

public sealed class ExpireAuditQueryHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireAuditQueryCommand)
            return;

        var aggregate = (AuditQueryAggregate)await context.LoadAggregateAsync(typeof(AuditQueryAggregate));
        aggregate.Expire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

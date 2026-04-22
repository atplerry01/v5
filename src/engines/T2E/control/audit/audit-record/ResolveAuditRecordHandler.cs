using Whycespace.Domain.ControlSystem.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditRecord;

public sealed class ResolveAuditRecordHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResolveAuditRecordCommand cmd)
            return;

        var aggregate = (AuditRecordAggregate)await context.LoadAggregateAsync(typeof(AuditRecordAggregate));
        aggregate.Resolve(cmd.ResolvedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

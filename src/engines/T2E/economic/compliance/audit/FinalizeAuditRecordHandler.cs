using Whycespace.Domain.EconomicSystem.Compliance.Audit;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Compliance.Audit;

public sealed class FinalizeAuditRecordHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FinalizeAuditRecordCommand cmd)
            return;

        var aggregate = (AuditRecordAggregate)await context.LoadAggregateAsync(typeof(AuditRecordAggregate));
        aggregate.FinalizeRecord(new Timestamp(cmd.FinalizedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

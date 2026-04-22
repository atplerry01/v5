using Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Constitutional.Chain.AnchorRecord;

public sealed class SealAnchorHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SealAnchorCommand cmd)
            return;

        var aggregate = (AnchorRecordAggregate)await context.LoadAggregateAsync(typeof(AnchorRecordAggregate));
        aggregate.Seal(cmd.SealedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

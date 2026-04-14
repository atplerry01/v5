using Whycespace.Domain.EconomicSystem.Capital.Reserve;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Reserve;

public sealed class ReleaseCapitalReserveHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReleaseCapitalReserveCommand cmd)
            return;

        var aggregate = (ReserveAggregate)await context.LoadAggregateAsync(typeof(ReserveAggregate));
        aggregate.Release(new Timestamp(cmd.ReleasedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

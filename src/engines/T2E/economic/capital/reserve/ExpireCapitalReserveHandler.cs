using Whycespace.Domain.EconomicSystem.Capital.Reserve;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Reserve;

public sealed class ExpireCapitalReserveHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireCapitalReserveCommand cmd)
            return;

        var aggregate = (ReserveAggregate)await context.LoadAggregateAsync(typeof(ReserveAggregate));
        aggregate.Expire(new Timestamp(cmd.ExpiredAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

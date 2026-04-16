using Whycespace.Domain.EconomicSystem.Exchange.Rate;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Exchange.Rate;

public sealed class ExpireExchangeRateHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireExchangeRateCommand cmd)
            return;

        var aggregate = (ExchangeRateAggregate)await context.LoadAggregateAsync(typeof(ExchangeRateAggregate));
        aggregate.Expire(new Timestamp(cmd.ExpiredAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

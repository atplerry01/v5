using Whycespace.Domain.EconomicSystem.Exchange.Rate;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Exchange.Rate;

public sealed class ActivateExchangeRateHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateExchangeRateCommand cmd)
            return;

        var aggregate = (ExchangeRateAggregate)await context.LoadAggregateAsync(typeof(ExchangeRateAggregate));
        aggregate.Activate(new Timestamp(cmd.ActivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

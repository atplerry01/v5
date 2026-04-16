using Whycespace.Domain.EconomicSystem.Exchange.Rate;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Exchange.Rate;

public sealed class DefineExchangeRateHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineExchangeRateCommand cmd)
            return Task.CompletedTask;

        var aggregate = ExchangeRateAggregate.DefineRate(
            new RateId(cmd.RateId),
            new Currency(cmd.BaseCurrency),
            new Currency(cmd.QuoteCurrency),
            cmd.RateValue,
            new Timestamp(cmd.EffectiveAt),
            cmd.Version);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

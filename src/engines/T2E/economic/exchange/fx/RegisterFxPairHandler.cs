using Whycespace.Domain.EconomicSystem.Exchange.Fx;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Exchange.Fx;

public sealed class RegisterFxPairHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterFxPairCommand cmd)
            return Task.CompletedTask;

        var aggregate = FxAggregate.Register(
            new FxId(cmd.FxId),
            new CurrencyPair(new Currency(cmd.BaseCurrency), new Currency(cmd.QuoteCurrency)));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

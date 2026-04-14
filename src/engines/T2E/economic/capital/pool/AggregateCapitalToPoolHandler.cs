using Whycespace.Domain.EconomicSystem.Capital.Pool;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Pool;

public sealed class AggregateCapitalToPoolHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AggregateCapitalToPoolCommand cmd)
            return;

        var aggregate = (CapitalPoolAggregate)await context.LoadAggregateAsync(typeof(CapitalPoolAggregate));
        aggregate.AggregateCapital(cmd.SourceAccountId, new Amount(cmd.Amount));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

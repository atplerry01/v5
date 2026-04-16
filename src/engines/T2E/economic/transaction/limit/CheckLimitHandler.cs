using Whycespace.Domain.EconomicSystem.Transaction.Limit;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Limit;

public sealed class CheckLimitHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CheckLimitCommand cmd)
            return;

        var aggregate = (LimitAggregate)await context.LoadAggregateAsync(typeof(LimitAggregate));

        aggregate.Check(cmd.TransactionId, new Amount(cmd.TransactionAmount), new Timestamp(cmd.CheckedAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}

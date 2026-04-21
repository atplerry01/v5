using Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderCore.LineItem;

public sealed class CancelLineItemHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CancelLineItemCommand)
            return;

        var aggregate = (LineItemAggregate)await context.LoadAggregateAsync(typeof(LineItemAggregate));
        aggregate.Cancel();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

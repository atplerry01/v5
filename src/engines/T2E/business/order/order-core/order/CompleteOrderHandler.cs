using Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderCore.Order;

public sealed class CompleteOrderHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteOrderCommand)
            return;

        var aggregate = (OrderAggregate)await context.LoadAggregateAsync(typeof(OrderAggregate));
        aggregate.Complete();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

using Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderCore.LineItem;

public sealed class UpdateLineItemQuantityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateLineItemQuantityCommand cmd)
            return;

        var aggregate = (LineItemAggregate)await context.LoadAggregateAsync(typeof(LineItemAggregate));
        aggregate.UpdateQuantity(new LineQuantity(cmd.QuantityValue, cmd.QuantityUnit));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

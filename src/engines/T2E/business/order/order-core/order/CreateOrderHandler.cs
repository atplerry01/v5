using Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderCore.Order;

public sealed class CreateOrderHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateOrderCommand cmd)
            return Task.CompletedTask;

        var aggregate = OrderAggregate.Create(
            new OrderId(cmd.OrderId),
            new OrderSourceReference(cmd.SourceReferenceId),
            new OrderDescription(cmd.Description));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

using Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderCore.LineItem;

public sealed class CreateLineItemHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateLineItemCommand cmd)
            return Task.CompletedTask;

        var aggregate = LineItemAggregate.Create(
            new LineItemId(cmd.LineItemId),
            new OrderRef(cmd.OrderId),
            new LineItemSubjectRef(
                (LineItemSubjectKind)cmd.SubjectKind,
                new LineItemSubjectId(cmd.SubjectId)),
            new LineQuantity(cmd.QuantityValue, cmd.QuantityUnit));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

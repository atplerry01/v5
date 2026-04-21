using Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderChange.FulfillmentInstruction;

public sealed class CreateFulfillmentInstructionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateFulfillmentInstructionCommand cmd)
            return Task.CompletedTask;

        LineItemRef? lineItem = cmd.LineItemId.HasValue
            ? new LineItemRef(cmd.LineItemId.Value)
            : null;

        var aggregate = FulfillmentInstructionAggregate.Create(
            new FulfillmentInstructionId(cmd.FulfillmentInstructionId),
            new OrderRef(cmd.OrderId),
            new FulfillmentDirective(cmd.Directive),
            lineItem);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

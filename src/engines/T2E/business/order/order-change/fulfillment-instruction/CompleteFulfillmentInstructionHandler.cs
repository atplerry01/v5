using Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderChange.FulfillmentInstruction;

public sealed class CompleteFulfillmentInstructionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteFulfillmentInstructionCommand cmd)
            return;

        var aggregate = (FulfillmentInstructionAggregate)await context.LoadAggregateAsync(typeof(FulfillmentInstructionAggregate));
        aggregate.Complete(cmd.CompletedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

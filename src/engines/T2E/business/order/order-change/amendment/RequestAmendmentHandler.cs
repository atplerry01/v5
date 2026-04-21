using Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderChange.Amendment;

public sealed class RequestAmendmentHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestAmendmentCommand cmd)
            return Task.CompletedTask;

        var aggregate = AmendmentAggregate.Request(
            new AmendmentId(cmd.AmendmentId),
            new OrderRef(cmd.OrderId),
            new AmendmentReason(cmd.Reason),
            cmd.RequestedAt);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

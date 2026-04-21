using Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderChange.Cancellation;

public sealed class RequestCancellationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestCancellationCommand cmd)
            return Task.CompletedTask;

        var aggregate = CancellationAggregate.Request(
            new CancellationId(cmd.CancellationId),
            new OrderRef(cmd.OrderId),
            new CancellationReason(cmd.Reason),
            cmd.RequestedAt);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

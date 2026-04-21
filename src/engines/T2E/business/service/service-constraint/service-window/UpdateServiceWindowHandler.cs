using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.ServiceWindow;

public sealed class UpdateServiceWindowHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateServiceWindowCommand cmd)
            return;

        var aggregate = (ServiceWindowAggregate)await context.LoadAggregateAsync(typeof(ServiceWindowAggregate));
        aggregate.UpdateRange(new TimeWindow(cmd.StartsAt, cmd.EndsAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.ServiceWindow;

public sealed class ActivateServiceWindowHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateServiceWindowCommand)
            return;

        var aggregate = (ServiceWindowAggregate)await context.LoadAggregateAsync(typeof(ServiceWindowAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

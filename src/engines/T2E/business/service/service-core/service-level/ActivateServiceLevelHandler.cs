using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceLevel;

public sealed class ActivateServiceLevelHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateServiceLevelCommand)
            return;

        var aggregate = (ServiceLevelAggregate)await context.LoadAggregateAsync(typeof(ServiceLevelAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

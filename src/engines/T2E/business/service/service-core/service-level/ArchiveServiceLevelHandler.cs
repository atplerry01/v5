using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceLevel;

public sealed class ArchiveServiceLevelHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveServiceLevelCommand)
            return;

        var aggregate = (ServiceLevelAggregate)await context.LoadAggregateAsync(typeof(ServiceLevelAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

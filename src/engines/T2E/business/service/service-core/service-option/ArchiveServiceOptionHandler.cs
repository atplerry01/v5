using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceOption;

public sealed class ArchiveServiceOptionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveServiceOptionCommand)
            return;

        var aggregate = (ServiceOptionAggregate)await context.LoadAggregateAsync(typeof(ServiceOptionAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

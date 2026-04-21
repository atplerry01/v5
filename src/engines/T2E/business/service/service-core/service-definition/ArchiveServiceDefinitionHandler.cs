using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceDefinition;

public sealed class ArchiveServiceDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveServiceDefinitionCommand)
            return;

        var aggregate = (ServiceDefinitionAggregate)await context.LoadAggregateAsync(typeof(ServiceDefinitionAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

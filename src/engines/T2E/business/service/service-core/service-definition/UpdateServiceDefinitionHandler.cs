using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceDefinition;

public sealed class UpdateServiceDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateServiceDefinitionCommand cmd)
            return;

        var aggregate = (ServiceDefinitionAggregate)await context.LoadAggregateAsync(typeof(ServiceDefinitionAggregate));
        aggregate.Update(new ServiceDefinitionName(cmd.Name), new ServiceCategory(cmd.Category));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

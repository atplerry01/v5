using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceDefinition;

public sealed class CreateServiceDefinitionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateServiceDefinitionCommand cmd)
            return Task.CompletedTask;

        var aggregate = ServiceDefinitionAggregate.Create(
            new ServiceDefinitionId(cmd.ServiceDefinitionId),
            new ServiceDefinitionName(cmd.Name),
            new ServiceCategory(cmd.Category));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

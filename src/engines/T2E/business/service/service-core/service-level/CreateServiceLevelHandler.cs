using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceLevel;

public sealed class CreateServiceLevelHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateServiceLevelCommand cmd)
            return Task.CompletedTask;

        var aggregate = ServiceLevelAggregate.Create(
            new ServiceLevelId(cmd.ServiceLevelId),
            new ServiceDefinitionRef(cmd.ServiceDefinitionId),
            new LevelCode(cmd.Code),
            new LevelName(cmd.Name),
            new ServiceLevelTarget(cmd.Target));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

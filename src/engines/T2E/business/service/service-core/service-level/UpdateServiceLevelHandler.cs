using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceLevel;

public sealed class UpdateServiceLevelHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateServiceLevelCommand cmd)
            return;

        var aggregate = (ServiceLevelAggregate)await context.LoadAggregateAsync(typeof(ServiceLevelAggregate));
        aggregate.Update(new LevelName(cmd.Name), new ServiceLevelTarget(cmd.Target));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

using Whycespace.Domain.EconomicSystem.Risk.Exposure;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Risk.Exposure;

public sealed class CloseRiskExposureHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CloseRiskExposureCommand)
            return;

        var aggregate = (ExposureAggregate)await context.LoadAggregateAsync(typeof(ExposureAggregate));
        aggregate.CloseExposure();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

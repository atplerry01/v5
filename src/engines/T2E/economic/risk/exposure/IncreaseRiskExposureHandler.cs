using Whycespace.Domain.DecisionSystem.Risk.Exposure;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Risk.Exposure;

public sealed class IncreaseRiskExposureHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not IncreaseRiskExposureCommand cmd)
            return;

        var aggregate = (ExposureAggregate)await context.LoadAggregateAsync(typeof(ExposureAggregate));
        aggregate.IncreaseExposure(new Amount(cmd.Amount));
        context.EmitEvents(aggregate.DomainEvents);
    }
}

using Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CommercialShape.Plan;

public sealed class DeprecatePlanHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecatePlanCommand)
            return;

        var aggregate = (PlanAggregate)await context.LoadAggregateAsync(typeof(PlanAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

using Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CommercialShape.Plan;

public sealed class ArchivePlanHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchivePlanCommand)
            return;

        var aggregate = (PlanAggregate)await context.LoadAggregateAsync(typeof(PlanAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

using Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CommercialShape.Plan;

public sealed class DraftPlanHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DraftPlanCommand cmd)
            return Task.CompletedTask;

        var aggregate = PlanAggregate.Draft(
            new PlanId(cmd.PlanId),
            new PlanDescriptor(cmd.PlanName, cmd.PlanTier));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

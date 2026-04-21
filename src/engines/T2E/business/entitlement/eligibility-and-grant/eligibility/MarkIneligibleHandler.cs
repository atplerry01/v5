using Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Eligibility;

public sealed class MarkIneligibleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkIneligibleCommand cmd)
            return;

        var aggregate = (EligibilityAggregate)await context.LoadAggregateAsync(typeof(EligibilityAggregate));
        aggregate.MarkIneligible(new IneligibilityReason(cmd.Reason), cmd.EvaluatedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

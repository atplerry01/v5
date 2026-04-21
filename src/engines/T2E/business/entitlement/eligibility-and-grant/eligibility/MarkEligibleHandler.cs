using Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Eligibility;

public sealed class MarkEligibleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkEligibleCommand cmd)
            return;

        var aggregate = (EligibilityAggregate)await context.LoadAggregateAsync(typeof(EligibilityAggregate));
        aggregate.MarkEligible(cmd.EvaluatedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

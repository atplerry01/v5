using Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Assignment;

public sealed class ActivateAssignmentHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateAssignmentCommand cmd)
            return;

        var aggregate = (AssignmentAggregate)await context.LoadAggregateAsync(typeof(AssignmentAggregate));
        aggregate.Activate(cmd.ActivatedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

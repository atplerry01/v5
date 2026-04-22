using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyAudit;

public sealed class ReviewPolicyAuditEntryHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReviewPolicyAuditEntryCommand cmd)
            return;

        var aggregate = (PolicyAuditAggregate)await context.LoadAggregateAsync(typeof(PolicyAuditAggregate));
        aggregate.Review(cmd.ReviewerId, cmd.Reason);
        context.EmitEvents(aggregate.DomainEvents);
    }
}

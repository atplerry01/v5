using Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Approval;

public sealed class RejectApprovalHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RejectApprovalCommand)
            return;

        var aggregate = (ApprovalAggregate)await context.LoadAggregateAsync(typeof(ApprovalAggregate));
        aggregate.Reject();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

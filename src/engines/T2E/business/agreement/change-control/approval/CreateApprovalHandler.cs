using Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Approval;

public sealed class CreateApprovalHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateApprovalCommand cmd)
            return Task.CompletedTask;

        var aggregate = ApprovalAggregate.Create(new ApprovalId(cmd.ApprovalId));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

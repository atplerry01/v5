using Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Amendment;

public sealed class CreateAmendmentHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateAmendmentCommand cmd)
            return Task.CompletedTask;

        var aggregate = AmendmentAggregate.Create(
            new AmendmentId(cmd.AmendmentId),
            new AmendmentTargetId(cmd.TargetId));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

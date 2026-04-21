using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Obligation;

public sealed class CreateObligationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateObligationCommand cmd)
            return Task.CompletedTask;

        var aggregate = ObligationAggregate.Create(new ObligationId(cmd.ObligationId));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

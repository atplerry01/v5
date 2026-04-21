using Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Amendment;

public sealed class ApplyAmendmentHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ApplyAmendmentCommand)
            return;

        var aggregate = (AmendmentAggregate)await context.LoadAggregateAsync(typeof(AmendmentAggregate));
        aggregate.ApplyAmendment();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

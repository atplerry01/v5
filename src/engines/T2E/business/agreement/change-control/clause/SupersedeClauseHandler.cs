using Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Clause;

public sealed class SupersedeClauseHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SupersedeClauseCommand)
            return;

        var aggregate = (ClauseAggregate)await context.LoadAggregateAsync(typeof(ClauseAggregate));
        aggregate.Supersede();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

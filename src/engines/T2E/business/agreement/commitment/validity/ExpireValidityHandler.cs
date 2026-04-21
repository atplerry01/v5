using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Validity;

public sealed class ExpireValidityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireValidityCommand)
            return;

        var aggregate = (ValidityAggregate)await context.LoadAggregateAsync(typeof(ValidityAggregate));
        aggregate.Expire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

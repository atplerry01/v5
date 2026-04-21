using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Contract;

public sealed class ActivateContractHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateContractCommand)
            return;

        var aggregate = (ContractAggregate)await context.LoadAggregateAsync(typeof(ContractAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Contract;

public sealed class TerminateContractHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not TerminateContractCommand)
            return;

        var aggregate = (ContractAggregate)await context.LoadAggregateAsync(typeof(ContractAggregate));
        aggregate.Terminate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}

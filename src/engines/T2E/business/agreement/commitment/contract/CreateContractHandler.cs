using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Contract;

public sealed class CreateContractHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateContractCommand cmd)
            return Task.CompletedTask;

        var aggregate = ContractAggregate.Create(new ContractId(cmd.ContractId), cmd.CreatedAt);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

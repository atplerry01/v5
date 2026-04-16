using Whycespace.Domain.EconomicSystem.Transaction.Instruction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Instruction;

public sealed class CancelInstructionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CancelInstructionCommand cmd)
            return;

        var aggregate = (TransactionInstructionAggregate)await context.LoadAggregateAsync(typeof(TransactionInstructionAggregate));

        aggregate.CancelInstruction(cmd.Reason, new Timestamp(cmd.CancelledAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}

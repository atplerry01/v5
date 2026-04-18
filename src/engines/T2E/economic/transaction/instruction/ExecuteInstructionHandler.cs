using Whycespace.Domain.EconomicSystem.Transaction.Instruction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;

using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Instruction;

public sealed class ExecuteInstructionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExecuteInstructionCommand cmd)
            return;

        EnforcementGuard.RequireNotRestricted(context.EnforcementConstraint, context.IsSystem);

        var aggregate = (TransactionInstructionAggregate)await context.LoadAggregateAsync(typeof(TransactionInstructionAggregate));

        aggregate.MarkExecuted(new Timestamp(cmd.ExecutedAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}

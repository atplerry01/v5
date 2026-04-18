using Whycespace.Domain.EconomicSystem.Transaction.Instruction;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;

using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Instruction;

public sealed class CreateInstructionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateInstructionCommand cmd)
            return Task.CompletedTask;

        EnforcementGuard.RequireNotRestricted(context.EnforcementConstraint, context.IsSystem);

        if (!Enum.TryParse<InstructionType>(cmd.Type, ignoreCase: true, out var instructionType))
            throw new ArgumentException($"Unknown instruction type '{cmd.Type}'.", nameof(cmd.Type));

        var aggregate = TransactionInstructionAggregate.CreateInstruction(
            InstructionId.From(cmd.InstructionId),
            cmd.FromAccountId,
            cmd.ToAccountId,
            new Amount(cmd.Amount),
            new Currency(cmd.Currency),
            instructionType,
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

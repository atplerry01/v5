using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Instruction;

namespace Whycespace.Projections.Economic.Transaction.Instruction.Reducer;

public static class InstructionProjectionReducer
{
    public static InstructionReadModel Apply(InstructionReadModel state, TransactionInstructionCreatedEventSchema e) =>
        state with
        {
            InstructionId = e.AggregateId,
            FromAccountId = e.FromAccountId,
            ToAccountId = e.ToAccountId,
            Amount = e.Amount,
            Currency = e.Currency,
            Type = e.Type,
            Status = "Pending",
            CreatedAt = e.CreatedAt
        };

    public static InstructionReadModel Apply(InstructionReadModel state, TransactionInstructionExecutedEventSchema e) =>
        state with
        {
            InstructionId = e.AggregateId,
            Status = "Executed",
            ExecutedAt = e.ExecutedAt
        };

    public static InstructionReadModel Apply(InstructionReadModel state, TransactionInstructionCancelledEventSchema e) =>
        state with
        {
            InstructionId = e.AggregateId,
            Status = "Cancelled",
            CancelledAt = e.CancelledAt,
            CancellationReason = e.Reason
        };
}

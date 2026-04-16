using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Transaction.Instruction;

public sealed record CreateInstructionCommand(
    Guid InstructionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    string Type,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => InstructionId;
}

public sealed record ExecuteInstructionCommand(
    Guid InstructionId,
    DateTimeOffset ExecutedAt) : IHasAggregateId
{
    public Guid AggregateId => InstructionId;
}

public sealed record CancelInstructionCommand(
    Guid InstructionId,
    string Reason,
    DateTimeOffset CancelledAt) : IHasAggregateId
{
    public Guid AggregateId => InstructionId;
}

namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public sealed class InstructionValidationService
{
    public bool ValidateInstruction(TransactionInstructionAggregate instruction) =>
        instruction.Amount.Value > 0m &&
        instruction.FromAccountId != Guid.Empty &&
        instruction.ToAccountId != Guid.Empty &&
        instruction.FromAccountId != instruction.ToAccountId;
}

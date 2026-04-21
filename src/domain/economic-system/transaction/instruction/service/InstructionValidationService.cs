namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public sealed class InstructionValidationService
{
    public bool ValidateInstruction(TransactionInstructionAggregate instruction) =>
        instruction.Amount.Value > 0m &&
        instruction.FromAccountId.Value != Guid.Empty &&
        instruction.ToAccountId.Value != Guid.Empty &&
        instruction.FromAccountId.Value != instruction.ToAccountId.Value;
}

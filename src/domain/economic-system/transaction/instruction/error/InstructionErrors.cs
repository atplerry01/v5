using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public static class InstructionErrors
{
    public static DomainException InvalidAmount() =>
        new("Amount must be greater than zero.");

    public static DomainException InvalidFromAccount() =>
        new("Instruction must specify a valid source account.");

    public static DomainException InvalidToAccount() =>
        new("Instruction must specify a valid destination account.");

    public static DomainException AccountsMustDiffer() =>
        new("Source and destination accounts must be different.");

    public static DomainException InstructionNotPending() =>
        new("Instruction must be in Pending status.");

    public static DomainException InstructionAlreadyExecuted() =>
        new("Instruction has already been executed.");

    public static DomainException InstructionAlreadyCancelled() =>
        new("Instruction has already been cancelled.");

    public static DomainException CannotExecuteCancelledInstruction() =>
        new("Cannot execute a cancelled instruction.");

    public static DomainException CannotCancelExecutedInstruction() =>
        new("Cannot cancel an executed instruction.");

    public static DomainInvariantViolationException NegativeAmount() =>
        new("Invariant violated: instruction amount cannot be negative.");
}

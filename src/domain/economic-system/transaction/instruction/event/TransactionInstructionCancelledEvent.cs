using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public sealed record TransactionInstructionCancelledEvent(
    InstructionId InstructionId,
    string Reason,
    Timestamp CancelledAt) : DomainEvent;

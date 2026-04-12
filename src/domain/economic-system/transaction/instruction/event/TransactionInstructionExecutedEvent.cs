using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public sealed record TransactionInstructionExecutedEvent(
    InstructionId InstructionId,
    Timestamp ExecutedAt) : DomainEvent;

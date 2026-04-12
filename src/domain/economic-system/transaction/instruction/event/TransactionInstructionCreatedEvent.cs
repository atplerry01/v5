using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public sealed record TransactionInstructionCreatedEvent(
    InstructionId InstructionId,
    Guid FromAccountId,
    Guid ToAccountId,
    Amount Amount,
    Currency Currency,
    InstructionType Type,
    Timestamp CreatedAt) : DomainEvent;

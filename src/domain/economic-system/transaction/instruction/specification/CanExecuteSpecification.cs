using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public sealed class CanExecuteSpecification : Specification<TransactionInstructionAggregate>
{
    public override bool IsSatisfiedBy(TransactionInstructionAggregate instruction) =>
        instruction.Status == InstructionStatus.Pending;
}

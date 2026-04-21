using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public readonly record struct FulfillmentInstructionId
{
    public Guid Value { get; }

    public FulfillmentInstructionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "FulfillmentInstructionId cannot be empty.");
        Value = value;
    }
}

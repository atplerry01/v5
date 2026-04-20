namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public readonly record struct FulfillmentInstructionId
{
    public Guid Value { get; }

    public FulfillmentInstructionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FulfillmentInstructionId value must not be empty.", nameof(value));

        Value = value;
    }
}

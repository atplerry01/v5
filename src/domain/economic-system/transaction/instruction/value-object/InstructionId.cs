namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public readonly record struct InstructionId
{
    public Guid Value { get; }

    public InstructionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("InstructionId cannot be empty.", nameof(value));
        Value = value;
    }

    public static InstructionId From(Guid value) => new(value);
}

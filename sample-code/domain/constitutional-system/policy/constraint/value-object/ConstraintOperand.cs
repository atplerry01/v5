namespace Whycespace.Domain.ConstitutionalSystem.Policy.Constraint;

using Whycespace.Domain.SharedKernel;

public sealed class ConstraintOperand : ValueObject
{
    public OperandKind Kind { get; }
    public string Value { get; }

    private ConstraintOperand(OperandKind kind, string value)
    {
        Kind = kind;
        Value = value;
    }

    public static ConstraintOperand Literal(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new ConstraintOperand(OperandKind.Literal, value);
    }

    public static ConstraintOperand FactReference(string factKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(factKey);
        return new ConstraintOperand(OperandKind.FactReference, factKey);
    }

    public object? Resolve(IReadOnlyDictionary<string, object> facts)
    {
        return Kind switch
        {
            OperandKind.Literal => Value,
            OperandKind.FactReference => facts.GetValueOrDefault(Value),
            _ => null
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Kind;
        yield return Value;
    }

    public override string ToString() => $"{Kind}:{Value}";
}

public enum OperandKind
{
    Literal,
    FactReference
}

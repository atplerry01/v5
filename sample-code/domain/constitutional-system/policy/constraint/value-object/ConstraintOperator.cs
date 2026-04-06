namespace Whycespace.Domain.ConstitutionalSystem.Policy.Constraint;

using Whycespace.Domain.SharedKernel;

public sealed class ConstraintOperator : ValueObject
{
    public string Value { get; }

    private ConstraintOperator(string value) => Value = value;

    public static readonly ConstraintOperator Equal = new("eq");
    public static readonly ConstraintOperator NotEqual = new("neq");
    public static readonly ConstraintOperator GreaterThan = new("gt");
    public static readonly ConstraintOperator GreaterThanOrEqual = new("gte");
    public static readonly ConstraintOperator LessThan = new("lt");
    public static readonly ConstraintOperator LessThanOrEqual = new("lte");
    public static readonly ConstraintOperator Contains = new("contains");
    public static readonly ConstraintOperator In = new("in");

    private static readonly Dictionary<string, ConstraintOperator> All = new()
    {
        [Equal.Value] = Equal,
        [NotEqual.Value] = NotEqual,
        [GreaterThan.Value] = GreaterThan,
        [GreaterThanOrEqual.Value] = GreaterThanOrEqual,
        [LessThan.Value] = LessThan,
        [LessThanOrEqual.Value] = LessThanOrEqual,
        [Contains.Value] = Contains,
        [In.Value] = In
    };

    public static ConstraintOperator From(string value)
    {
        if (!All.TryGetValue(value, out var op))
            throw new ArgumentException($"Unknown constraint operator: {value}");
        return op;
    }

    public bool Apply(object? left, object? right)
    {
        if (left is null || right is null) return false;

        if (left is IComparable comparableLeft && right is IComparable comparableRight)
        {
            var comparison = comparableLeft.CompareTo(comparableRight);
            if (this == Equal) return comparison == 0;
            if (this == NotEqual) return comparison != 0;
            if (this == GreaterThan) return comparison > 0;
            if (this == GreaterThanOrEqual) return comparison >= 0;
            if (this == LessThan) return comparison < 0;
            if (this == LessThanOrEqual) return comparison <= 0;
        }

        if (this == Equal) return Equals(left, right);
        if (this == NotEqual) return !Equals(left, right);
        if (this == Contains && left is string s) return s.Contains(right.ToString()!, StringComparison.Ordinal);
        if (this == In && right is IEnumerable<object> collection) return collection.Contains(left);

        return false;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

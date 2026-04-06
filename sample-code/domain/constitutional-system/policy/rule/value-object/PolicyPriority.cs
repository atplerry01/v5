namespace Whycespace.Domain.ConstitutionalSystem.Policy;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyPriority : ValueObject
{
    public int Value { get; }

    private PolicyPriority(int value) => Value = value;

    public static PolicyPriority Default => new(0);

    public static PolicyPriority From(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Priority must be non-negative.");
        return new PolicyPriority(value);
    }

    public bool IsHigherThan(PolicyPriority other) => Value > other.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}

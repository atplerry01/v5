using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

/// <summary>
/// Defines the ordering guarantee level for event sequences.
/// </summary>
public sealed class OrderingGuarantee : ValueObject
{
    public static readonly OrderingGuarantee Strict = new("Strict");
    public static readonly OrderingGuarantee Causal = new("Causal");
    public static readonly OrderingGuarantee BestEffort = new("BestEffort");

    public string Value { get; }

    private OrderingGuarantee(string value) => Value = value;

    public bool IsStrict => this == Strict;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

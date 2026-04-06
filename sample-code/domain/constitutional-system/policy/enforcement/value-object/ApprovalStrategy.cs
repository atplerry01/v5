namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class ApprovalStrategy : ValueObject
{
    public string Value { get; }
    private ApprovalStrategy(string value) => Value = value;

    public static readonly ApprovalStrategy All = new("all");
    public static readonly ApprovalStrategy Majority = new("majority");
    public static readonly ApprovalStrategy Weighted = new("weighted");

    public static ApprovalStrategy From(string value) => new(value.ToLowerInvariant());

    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyChangeType : ValueObject
{
    public string Value { get; }
    private PolicyChangeType(string value) => Value = value;

    public static readonly PolicyChangeType Create = new("create");
    public static readonly PolicyChangeType Update = new("update");
    public static readonly PolicyChangeType Deprecate = new("deprecate");

    public static PolicyChangeType From(string value) => new(value.ToLowerInvariant());

    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}

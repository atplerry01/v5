namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class ApproverRole : ValueObject
{
    public string Value { get; }
    private ApproverRole(string value) => Value = value;

    public static readonly ApproverRole Guardian = new("guardian");
    public static readonly ApproverRole Auditor = new("auditor");
    public static readonly ApproverRole System = new("system");

    public static ApproverRole From(string value) => new(value.ToLowerInvariant());

    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}

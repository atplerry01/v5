namespace Whycespace.Domain.ContentSystem.Governance.ContentPolicy;

public readonly record struct ContentPolicyId(Guid Value)
{
    public static ContentPolicyId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}

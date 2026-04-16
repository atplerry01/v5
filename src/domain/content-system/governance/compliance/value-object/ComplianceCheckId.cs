namespace Whycespace.Domain.ContentSystem.Governance.Compliance;

public readonly record struct ComplianceCheckId(Guid Value)
{
    public static ComplianceCheckId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}

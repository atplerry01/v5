using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Compliance;

public sealed record ComplianceRuleRef : ValueObject
{
    public string Value { get; }
    private ComplianceRuleRef(string value) => Value = value;
    public static ComplianceRuleRef Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw ComplianceErrors.InvalidRule();
        return new ComplianceRuleRef(value.Trim());
    }
    public override string ToString() => Value;
}

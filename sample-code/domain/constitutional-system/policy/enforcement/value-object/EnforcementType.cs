namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

using Whycespace.Domain.SharedKernel;

public sealed class EnforcementType : ValueObject
{
    public string Value { get; }

    private EnforcementType(string value) => Value = value;

    public static readonly EnforcementType Warning = new("warning");
    public static readonly EnforcementType Restriction = new("restriction");
    public static readonly EnforcementType Freeze = new("freeze");
    public static readonly EnforcementType Block = new("block");
    public static readonly EnforcementType Halt = new("halt");
    public static readonly EnforcementType AuditTrigger = new("audit_trigger");

    private static readonly Dictionary<string, EnforcementType> All = new()
    {
        [Warning.Value] = Warning,
        [Restriction.Value] = Restriction,
        [Freeze.Value] = Freeze,
        [Block.Value] = Block,
        [Halt.Value] = Halt,
        [AuditTrigger.Value] = AuditTrigger
    };

    public static EnforcementType From(string value)
    {
        if (!All.TryGetValue(value, out var type))
            throw new ArgumentException($"Unknown enforcement type: {value}");
        return type;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
